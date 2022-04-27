//Author: Marlon Robancho
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;

namespace System_Dependencies
{
    internal class VM : INotifyPropertyChanged
    {
        public BindingList<string> Results { get; set; } = new BindingList<string>();
        public List<Program> Programs = new List<Program>();
        List<string> lineProgramList;
        const string DEPENDCOMMAND = "DEPEND";
        const string INSTALLCOMMAND = "INSTALL";
        const string REMOVECOMMAND = "REMOVE";
        const string LISTCOMMAND = "LIST";
        const string ENDCOMMAND = "END";
        const int maxLineCommandLenght = 80;

        public void Init()
        {
            string text = File.ReadAllText("SystemDependenciesInput.txt");
            string[] lines = text.Split(new char[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            int lineNumber = 0;
            string errorInFile = "";
            bool isEndCommandExist = false;
            bool isInstalleCommandExecuted = false;

            Results.Clear();
            Programs.Clear();
            foreach (string line in lines)
            {
                lineNumber++;
                if (line.Length > maxLineCommandLenght)
                    errorInFile = "ERROR in line " + lineNumber + " : characters in a line can not be more than " + maxLineCommandLenght;
                string[] props = line.Split(new char[] { ' ' });
                lineProgramList = new List<string>();
                
                for (int i = 0; i < props.Length; i++)
                {
                    if (!string.IsNullOrEmpty(props[i].Trim()))
                    {
                        lineProgramList.Add(props[i]);
                    }
                }
                if (lineProgramList[0] == DEPENDCOMMAND && isInstalleCommandExecuted)
                    errorInFile += (errorInFile == "" ? "" : "\n") + "ERROR in line " + lineNumber + " : DEPEND command can NOT be exceuted after the INSTALL command";

                if (lineProgramList[0] == DEPENDCOMMAND && !isInstalleCommandExecuted)
                {
                    List<string> ProgramsThisDependsOn = new();
                    for (int j = 1; j < lineProgramList.Count; j++)
                    {
                        //Start on 1
                        string programsThisDependsOn = j + 1 < lineProgramList.Count && j >= 1 ? lineProgramList[j + 1] : null;
                        if (programsThisDependsOn != null)
                            ProgramsThisDependsOn.Add(programsThisDependsOn);
                    }
                    Programs.Add(
                        new Program
                        {
                            Name = lineProgramList[1],
                            ProgramsThisDependsOn = ProgramsThisDependsOn
                        }
                    );

                    for (int k = 2; k < lineProgramList.Count; k++)
                    {
                        Program p = getProgram(lineProgramList[k]);
                        if (p != null)
                            p.ProgramsDependsOnThis.Add(lineProgramList[1]);
                        else
                        {
                            Programs.Add(
                                new Program {Name = lineProgramList[k], ProgramsDependsOnThis = new List<string> { lineProgramList[1] }}
                            );
                        }

                    }

                    Results.Add(line);
                }

                if (lineProgramList[0] == INSTALLCOMMAND)
                {
                    isInstalleCommandExecuted = true;
                    Program p = getProgram(lineProgramList[1]);
                    if(p == null)
                    {
                        Programs.Add(new Program {Name = lineProgramList[1], isManuallyInstalled = true});
                    }
                    Results.Add(line);
                    Results.Add(InstallProgram(lineProgramList[1], true));
                }

                if (lineProgramList[0] == REMOVECOMMAND)
                {
                    Results.Add(line);
                    Results.Add(RemoveProgram(lineProgramList[1], true));
                }

                if (lineProgramList[0] == LISTCOMMAND)
                {
                    Results.Add(line);
                    foreach (Program p in Programs)
                    {
                        if (p.isInstalled)
                            Results.Add("   " + p.Name);
                    }
                }

                if (lineProgramList[0] == ENDCOMMAND)
                {
                    isEndCommandExist = true;
                    Results.Add(line);
                }

                if (!isEndCommandExist && lineNumber == lines.Length)
                    errorInFile += (errorInFile == "" ? "" : "\n") + "ERROR: END Command NOT FOUND";

                if (errorInFile != "")
                {
                    Results.Clear();
                    Results.Add(errorInFile);
                }
            }
        }

        //FirstOrDefault is not available
        private Program getProgram(string Name)
        {
            Program p = null;
            for (int i = 0; i < Programs.Count; i++)
            {
                if (Programs[i].Name == Name)
                {
                    p = Programs[i];
                    break;
                }
            }

            return p;
        }

        public string InstallProgram(string name, bool isManuallyInstalled)
        {
            string echo = "";

            Program program = getProgram(name);

            if (program != null)
            {
                if (!program.isInstalled)
                {
                    if (isManuallyInstalled)
                        program.isManuallyInstalled = true;

                    if (program.ProgramsThisDependsOn != null && program.ProgramsThisDependsOn.Count > 0)
                    {
                        foreach (string programName in program.ProgramsThisDependsOn)
                        {
                            echo += InstallProgram(programName, false);
                        }
                    }

                    string nextline = echo == "" ? "" : "\n";
                    echo += nextline + "    Installing "  + name;
                    program.isInstalled = true;
                }
                else if (isManuallyInstalled)
                    echo = "    " + name + " is already installed.";
            }

            return echo;
        }

        public string RemoveProgram(string name, bool isManuallyRemoved)
        {
            string echo = "";
            string errorInUse = "IN USE";
            bool canRemoved = false;
            Program program = getProgram(name);

            if (program != null)
            {
                if (isManuallyRemoved && program.isManuallyInstalled)
                    canRemoved = true;
                if (program.ProgramsDependsOnThis != null && program.ProgramsDependsOnThis.Count > 0)
                {
                    foreach (string programName in program.ProgramsDependsOnThis)
                    {
                        if (RemoveProgram(programName, false) == errorInUse)
                        {
                            canRemoved = false;
                            break;
                        }
                        else
                            canRemoved = true;
                    }

                }
                else if (!program.isInstalled)
                {
                    canRemoved = true;
                }

                if (!canRemoved)
                    echo = errorInUse;
                if (isManuallyRemoved && !program.isInstalled)
                {
                    echo = "    " + name + " is not installed.";
                }
                else if (isManuallyRemoved && canRemoved)
                {
                    program.isInstalled = false;
                    echo += "   Removing " + name;
                }
                else if (isManuallyRemoved && !canRemoved)
                {
                    echo = "   " + name + " is still needed.";
                }

                if (echo == "" && canRemoved)
                    program.isInstalled = false;

                if (isManuallyRemoved && canRemoved)
                {
                    if (program.ProgramsThisDependsOn != null && program.ProgramsThisDependsOn.Count > 0)
                    {
                        foreach (string programName in program.ProgramsThisDependsOn)
                        {
                            if (RemoveProgram(programName, false) == errorInUse)
                            {
                                canRemoved = false;
                            }
                            else
                            {
                                program.isInstalled = false;
                                echo += echo == "" ? "" : "\n" + "   Removing " + programName;
                            }
                        }
                    }
                }
            }
            return echo;
        }

        #region prop change
        public event PropertyChangedEventHandler PropertyChanged;

        private void propChange([CallerMemberName] string propertyName = "")
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        #endregion

    }
}
