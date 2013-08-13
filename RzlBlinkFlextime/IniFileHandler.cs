using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Xml;

namespace RzlBlinkFlextime
{

    /// <summary>
    /// Klasse, um Dateien im Ini-Format zu verwalten.
    /// </summary>
    public class IniFileHandler
    {
        /// <summary>
        /// Inhalt der Datei
        /// </summary>
        private List<string> lines = new List<string>();

        /// <summary>
        /// Anzahl der Zeilen in der INIDatei
        /// </summary>
        public int LineCount
        {
            get { return lines.Count; }
        }

        /// <summary>
        /// Verzeichnis der Datei
        /// </summary>
        /// <returns></returns>
        public string FileDirectory
        {
            get { return Path.GetDirectoryName(FileName); }
        }

        /// <summary>
        /// Voller Name der Datei bzw Titel der INI
        /// </summary>
        /// <returns></returns>
        public string FileName
        {
            get { return fileName; }
            set { this.fileName = value; }
        }

        /// <summary>
        /// Voller Pfad und Name der Datei bzw Titel der INI
        /// </summary>
        private string fileName = "";

        /// <summary>
        /// Gibt an, welche Zeichen als Kommentarbeginn
        /// gewertet werden sollen. Dabei wird das erste
        /// Zeichen defaultm��ig f�r neue Kommentare
        /// verwendet.
        /// </summary>
        private string CommentCharacters = "#;";

        /// <summary>
        /// Regul�rer Ausdruck f�r einen Kommentar in einer Zeile
        /// </summary>
        private string regCommentStr = "";

        /// <summary>
        /// Regul�rer Ausdruck f�r einen Eintrag
        /// </summary>
        private Regex regEntry = null;

        /// <summary>
        /// Regul�rer Ausdruck f�r einen Bereichskopf
        /// </summary>
        private Regex regCaption = null;

        /// <summary>
        /// �nderrungen an einem Eintrag vornemen. Achtung CaseSensitive!
        /// </summary>
        public string this[string Caption, string Entry]
        {
            get
            {
                return getValue(Caption, Entry, true);
            }
            set
            {
                setValue(Caption, Entry, value.ToString(), true, false);
            }
        }

        /// <summary>
        /// Leerer Standard-Konstruktor
        /// </summary>
        public IniFileHandler()
        {
            regCommentStr = @"(\s*[" + CommentCharacters + "](?<comment>.*))?";
            regEntry = new Regex(@"^[ \t]*(?<entry>([^=])+)=(?<value>([^=" + CommentCharacters + "])+)" + regCommentStr + "$");
            regCaption = new Regex(@"^[ \t]*(\[(?<caption>([^\]])+)\]){1}" + regCommentStr + "$");
        }

        /// <summary>
        /// Konstruktor, welcher sofort eine Datei einliest
        /// </summary>
        /// <param name="filename">Name der einzulesenden Datei</param>
        public IniFileHandler(string Path)
            : this()
        {
            if (!File.Exists(Path))
                throw new IOException("File " + Path + "  not found");
            else
            {
                FileName = Path;
                Read(File.ReadAllText(FileName));
            }

        }

        /// <summary>
        /// Methode die einen string im INI-Format einlie�t
        /// </summary>
        /// <param name="iniFile">String im INI-Format</param>
        public void Read(string iniFile)
        {
            lines.AddRange(iniFile.Split(new string[] { "\r\n", "\n" }, StringSplitOptions.None));
        }

        /// <summary>
        /// Methode die einen string im INI-Format einlie�t
        /// </summary>
        /// <param name="iniFile">String im INI-Format</param>
        public void ReadFile(string iniFile)
        {
            Read(File.ReadAllText(iniFile));
        }

        /// <summary>
        /// Kopie der Datei speichern (Objekt bleibt unber�hrt)
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        public bool SaveAs(string file)
        {
            if (fileName == "") return false;
            try
            {
                File.WriteAllLines(fileName, lines.ToArray());
            }
            catch (IOException ex)
            {
                throw new IOException("Fehler beim Schreiben der Datei " + fileName, ex);
            }
            catch (Exception ex)
            {
                throw new IOException("Fehler beim Schreiben der Datei " + fileName, ex);
            }
            return true;
        }

        /// <summary>
        /// Datei speichern
        /// </summary>
        /// <returns></returns>
        public bool Save()
        {
            return SaveAs(fileName);
        }

        /// <summary>
        /// Datei unter anderrem Namen speichern
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        public bool Save(string file)
        {
            fileName = file;
            return SaveAs(fileName);
        }

        /// <summary>
        /// Sucht die Zeilennummer (nullbasiert)
        /// eines gew�nschten Eintrages
        /// </summary>
        /// <param name="Caption">Name des Bereiches</param>
        /// <param name="CaseSensitive">true = Gross-/Kleinschreibung beachten</param>
        /// <returns>Nummer der Zeile, sonst -1</returns>
        public int GetCaptionLine(string Caption, bool CaseSensitive)
        {
            if (!CaseSensitive) Caption = Caption.ToLower();
            for (int i = 0; i < lines.Count; i++)
            {
                string line = lines[i].Trim();
                if (line == "") continue;
                if (!CaseSensitive) line = line.ToLower();
                // Erst den gew�nschten Abschnitt suchen
                if (line == "[" + Caption + "]")
                    return i;
            }
            return -1;// Bereich nicht gefunden
        }

        /// <summary>
        /// Pr�ft ob ein Bereich existiert.
        /// </summary>
        /// <param name="Caption">Name des Bereiches</param>
        /// <param name="CaseSensitive">true = Gross-/Kleinschreibung beachten</param>
        /// <returns>True wenn der Breich existiert, sonst false</returns>
        public bool CaptionExist(string Caption, bool CaseSensitive)
        {
            return GetCaptionLine(Caption, CaseSensitive) != -1;
        }

        /// <summary>
        /// Sucht die Zeilennummer (nullbasiert)
        /// eines gew�nschten Eintrages
        /// </summary>
        /// <param name="Caption">Name des Bereiches</param>
        /// <param name="Entry">Name des Eintrages</param>
        /// <param name="CaseSensitive">true = Gross-/Kleinschreibung beachten</param>
        /// <returns>Nummer der Zeile, sonst -1</returns>
        public int GetEntryLine(string Caption, string Entry, bool CaseSensitive)
        {
            Caption = Caption.ToLower();
            if (!CaseSensitive) Entry = Entry.ToLower();
            int CaptionStart = GetCaptionLine(Caption, false);
            if (CaptionStart < 0) return -1;
            for (int i = CaptionStart + 1; i < lines.Count; i++)
            {
                string line = lines[i].Trim();
                if (line == "") continue;
                if (!CaseSensitive) line = line.ToLower();
                if (line.StartsWith("["))
                    return -1;// Ende, wenn der n�chste Abschnitt beginnt
                if (Regex.IsMatch(line, @"^[ \t]*[" + CommentCharacters + "]"))
                    continue; // Kommentar
                if (line.StartsWith(Entry + "="))
                    return i;// Eintrag gefunden
            }
            return -1;// Eintrag nicht gefunden
        }

        /// <summary>
        /// Pr�ft ob ein Eintrag existiert.
        /// </summary>
        /// <param name="Caption">Name des Bereiches in dem gesucht werden soll</param>
        /// <param name="Entry">Name des Eintrag der gesucht werden soll</param>
        /// <param name="CaseSensitive">true = Gross-/Kleinschreibung beachten</param>
        /// <returns>True wenn der Eintrag existiert, sonst false</returns>
        public bool EntryExist(string Caption, string Entry, bool CaseSensitive)
        {
            return GetCaptionLine(Caption, CaseSensitive) != -1;
        }

        /// <summary>
        /// Kommentiert einen Wert aus
        /// </summary>
        /// <param name="Caption">Name des Bereiches</param>
        /// <param name="Entry">Name des Eintrages</param>
        /// <param name="CaseSensitive">true = Gross-/Kleinschreibung beachten</param>
        /// <returns>true = Eintrag gefunden und auskommentiert</returns>
        public bool commentValue(string Caption, string Entry, bool CaseSensitive)
        {
            int line = GetEntryLine(Caption, Entry, CaseSensitive);
            if (line < 0)
                return false;
            lines[line] = CommentCharacters[0] + lines[line];
            return true;
        }

        /// <summary>
        /// L�scht einen Wert
        /// </summary>
        /// <param name="Caption">Name des Bereiches</param>
        /// <param name="Entry">Name des Eintrages</param>
        /// <param name="CaseSensitive">true = Gross-/Kleinschreibung beachten</param>
        /// <returns>true = Eintrag gefunden und gel�scht</returns>
        public bool deleteValue(string Caption, string Entry, bool CaseSensitive)
        {
            int line = GetEntryLine(Caption, Entry, CaseSensitive);
            if (line < 0)
                return false;
            lines.RemoveAt(line);
            return true;
        }

        /// <summary>
        /// Liest den Wert eines Eintrages aus
        /// </summary>
        /// <param name="Caption">Name des Bereiches</param>
        /// <param name="Entry">Name des Eintrages</param>
        /// <param name="CaseSensitive">true = Gross-/Kleinschreibung beachten</param>
        /// <returns>Wert des Eintrags oder leer</returns>
        public string getValue(string Caption, string Entry, bool CaseSensitive)
        {
            int line = GetEntryLine(Caption, Entry, CaseSensitive);
            if (line < 0)
                return "";
            int pos = lines[line].IndexOf("=");
            if (pos < 0)
                return "";
            return lines[line].Substring(pos + 1).Trim();
            // Evtl. noch abschliessende Kommentarbereiche entfernen
        }

        /// <summary>
        /// Liest den Wert eines Eintrages aus
        /// Ignoriert Gross-/Kleinschreibung
        /// </summary>
        /// <param name="Caption">Name des Bereiches</param>
        /// <param name="Entry">Name des Eintrages</param>
        /// <returns>Wert des Eintrags oder leer</returns>
        public string getValue(string Caption, string Entry)
        {
            return getValue(Caption, Entry, false);
        }

        public bool getBoolValue(string Caption, string Entry)
        {
            string strValue = getValue(Caption, Entry, false);
            strValue = strValue.ToLower();

            if (string.Equals(strValue, "true")) return true;
            if (string.Equals(strValue, "1")) return true;
            if (string.Equals(strValue, "ja")) return true;
            if (string.Equals(strValue, "j")) return true;
            if (string.Equals(strValue, "y")) return true;

            return false;
        }

        /// <summary>
        /// Setzt einen Wert in einem Bereich. Wenn der Wert
        /// (und der Bereich) noch nicht existiert, werden die
        /// entsprechenden Eintr�ge erstellt.
        /// </summary>
        /// <param name="Caption">Name des Bereichs</param>
        /// <param name="Entry">name des Eintrags</param>
        /// <param name="Value">Wert des Eintrags</param>
        /// <param name="CaseSensitive">true = Gross-/Kleinschreibung beachten</param>
        /// <returns>true = Eintrag erfolgreich gesetzt</returns>
        public bool setValue(string Caption, string Entry, string Value, bool CaseSensitive, bool SearchComments)
        {
            Caption = Caption.ToLower();
            if (!CaseSensitive)
                Entry = Entry.ToLower();
            int lastCommentedFound = -1;
            int CaptionStart = GetCaptionLine(Caption, false);
            if (CaptionStart < 0)
            {
                lines.Add("[" + Caption + "]");
                lines.Add(Entry + "=" + Value);
                return true;
            }
            int EntryLine = GetEntryLine(Caption, Entry, CaseSensitive);
            for (int i = CaptionStart + 1; i < lines.Count; i++)
            {
                string line = lines[i].Trim();
                if (!CaseSensitive)
                    line = line.ToLower();
                if (line == "")
                    continue;
                // Ende, wenn der n�chste Abschnitt beginnt
                if (line.StartsWith("["))
                {
                    lines.Insert(i, Entry + "=" + Value);
                    return true;
                }
                // Suche aukommentierte, aber gesuchte Eintr�ge,
                // falls der Eintrag noch nicht existiert.
                if (SearchComments && EntryLine < 0 && Regex.IsMatch(line, @"^[ \t]*[" + CommentCharacters + "]"))
                {
                    string tmpLine = line.Substring(1).Trim();
                    if (tmpLine.StartsWith(Entry))
                    {
                        // Werte vergleichen, wenn gleich,
                        // nur Kommentarzeichen l�schen
                        int pos = tmpLine.IndexOf("=");
                        if (pos > 0)
                        {
                            if (Value == tmpLine.Substring(pos + 1).Trim())
                            {
                                lines[i] = tmpLine;
                                return true;
                            }
                        }
                        lastCommentedFound = i;
                    }
                    continue;// Kommentar
                }
                if (line.StartsWith(Entry))
                {
                    lines[i] = Entry + "=" + Value;
                    return true;
                }
            }
            if (lastCommentedFound > 0)
                lines.Insert(lastCommentedFound + 1, Entry + "=" + Value);
            else
                lines.Insert(CaptionStart + 1, Entry + "=" + Value);
            return true;
        }

        /// <summary>
        /// Setzt einen Wert in einem Bereich. Wenn der Wert
        /// (und der Bereich) noch nicht existiert, werden die
        /// entsprechenden Eintr�ge erstellt.
        /// </summary>
        /// <param name="Caption">Name des Bereichs</param>
        /// <param name="Entry">name des Eintrags</param>
        /// <param name="Value">Wert des Eintrags</param>
        /// <param name="CaseSensitive">true = Gross-/Kleinschreibung beachten</param>
        /// <returns>true = Eintrag erfolgreich gesetzt</returns>
        public bool setValue(string Caption, string Entry, string Value)
        {
            return setValue(Caption, Entry, Value, false, false);
        }

        /// <summary>
        /// Liest alle Eintr�ge uns deren Werte eines Bereiches aus
        /// </summary>
        /// <param name="Caption">Name des Bereichs</param>
        /// <param name="CaseSensitive">true = Gross-/Kleinschreibung beachten</param>
        /// <returns>Sortierte Liste mit Eintr�gen und Werten</returns>
        public SortedList<string, string> getCaption(string Caption, bool caseSensitive)
        {
            SortedList<string, string> result = new SortedList<string, string>();
            bool CaptionFound = false;
            for (int i = 0; i < lines.Count; i++)
            {
                string line = lines[i].Trim();
                if (line == "")
                    continue;

                if (caseSensitive)
                    Caption.ToLower();

                // Erst den gew�nschten Abschnitt suchen
                if (!CaptionFound)
                {
                    if (caseSensitive)
                        line.ToLower();

                    if (line != "[" + Caption + "]")
                    {
                        continue;
                    }
                    else
                    {
                        CaptionFound = true;
                        continue;
                    }
                }

                if (line.StartsWith("[")) // Ende, wenn der n�chste Abschnitt beginnt
                    break;
                if (Regex.IsMatch(line, @"^[ \t]*[" + CommentCharacters + "]")) // Kommentar
                    continue;
                int pos = line.IndexOf("=");
                if (pos < 0)
                    result.Add(line, "");
                else
                    result.Add(line.Substring(0, pos).Trim(), line.Substring(pos + 1).Trim());
            }
            return result;
        }

        /// <summary>
        /// Liest alle Eintr�ge uns deren Werte eines Bereiches aus.
        /// Ignoriert Gross-/Kleinschreibung
        /// </summary>
        /// <param name="Caption">Name des Bereichs</param>
        /// <param name="CaseSensitive">true = Gross-/Kleinschreibung beachten</param>
        /// <returns>Sortierte Liste mit Eintr�gen und Werten</returns>
        public SortedList<string, string> getCaption(string Caption)
        {
            return getCaption(Caption, false);
        }

        /// <summary>
        /// Erstellt eine Liste aller enthaltenen Bereiche
        /// </summary>
        /// <returns>Liste mit gefundenen Bereichen</returns>
        public List<string> getAllCaptions()
        {
            List<string> result = new List<string>();
            for (int i = 0; i < lines.Count; i++)
            {
                string line = lines[i];
                Match mCaption = regCaption.Match(lines[i]);
                if (mCaption.Success)
                    result.Add(mCaption.Groups["caption"].Value.Trim());
            }
            return result;
        }

        /// <summary>
        /// Exportiert s�mtliche Bereiche und deren Werte in ein XML-Dokument
        /// </summary>
        /// <returns>XML-Dokument</returns>
        public XmlDocument exportToXml()
        {
            XmlDocument doc = new XmlDocument();
            XmlElement root = null;

            root = doc.CreateElement(Path.GetFileNameWithoutExtension(this.fileName));

            doc.AppendChild(root);
            XmlElement Caption = null;

            for (int i = 0; i < lines.Count; i++)
            {
                Match mEntry = regEntry.Match(lines[i]);
                Match mCaption = regCaption.Match(lines[i]);
                if (mCaption.Success)
                {
                    Caption = doc.CreateElement(mCaption.Groups["caption"].Value.Trim());
                    root.AppendChild(Caption);
                    continue;
                }

                if (mEntry.Success)
                {
                    XmlElement xe = doc.CreateElement(mEntry.Groups["entry"].Value.Trim());
                    xe.InnerXml = mEntry.Groups["value"].Value.Trim();
                    if (Caption == null)
                        root.AppendChild(xe);
                    else
                        Caption.AppendChild(xe);
                }
            }
            return doc;
        }
    }

}

