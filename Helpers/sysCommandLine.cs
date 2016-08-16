///////////////////////////////////////////////////////////////////////////////
// Copyright (c) 2005-2016 Laszlo Arvai. All rights reserved.
//
// This program is free software; you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation; either version 3 of the License, or
// (at your option) any later version.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with this program; if not, write to the Free Software Foundation,
// Inc., 51 Franklin Street, Fifth Floor, Boston, MA 02110-1301  USA
///////////////////////////////////////////////////////////////////////////////
// File description
// ----------------
// Command line processor class
///////////////////////////////////////////////////////////////////////////////
using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.IO;

public class sysCommandLine
{
  #region Types

  public struct CommandLineParameters
  {
    public string Command;
    public string Parameter;
    public string Identifier;
		public string[] Options;
    public bool Used;

    public override bool Equals(object obj)
    {
      return Command.ToLower().CompareTo(((CommandLineParameters)obj).Command) == 0;
    }

    public override int GetHashCode()
    {
      return Command.ToLower().GetHashCode();
    }
  }

  struct CommandFileInfo
  {
    public string Name;
    public StreamReader File;

    public override bool Equals(object obj)
    {
      return Name.CompareTo(((CommandFileInfo)obj).Name) == 0;
    }

    public override int GetHashCode()
    {
      return Name.GetHashCode();
    }
  }

  #endregion

  #region Member variables
  CommandLineParameters[] m_parameters;
  CommandFileInfo[] m_command_file_info;
  public string ErrorMessage;

  #endregion

  public CommandLineParameters[] Parameters
  {
    get
    {
      return m_parameters;
    }
  }

  /// <summary>
  /// Process command line parameters
  /// </summary>
  /// <param name="in_command_line"></param>
  public bool ProcessCommandLine(string[] in_command_line)
  {
    int i;
    bool success = true;
    string original_path = Directory.GetCurrentDirectory();
    
    ErrorMessage = "";
    m_parameters = new CommandLineParameters[0];
    m_command_file_info = new CommandFileInfo[0];
    i = 0;
    // walk thru command line parameters
    while ((i < in_command_line.Length || m_command_file_info.Length > 0) && success)
    {
      if (m_command_file_info.Length > 0)
      {
        string buffer;

        try
        {
          buffer = m_command_file_info[m_command_file_info.Length - 1].File.ReadLine();

          // check eof
          if (buffer == null)
          {
            // close file
            m_command_file_info[m_command_file_info.Length - 1].File.Close();

            // remove from list
            Array.Resize(ref m_command_file_info, m_command_file_info.Length - 1);

            // restore previous directory
            if (m_command_file_info.Length > 0)
              ChangeDirectory(m_command_file_info[m_command_file_info.Length - 1].Name);
            else
              Directory.SetCurrentDirectory(original_path);
          }
          else
          {
            // trim buffer
            buffer = buffer.Trim();

            // skip empty lines
            if (buffer.Length > 0)
            {
              // skip comment line
              if (buffer[0] != ';')
              {
                success = ProcessCommandLineArgument(buffer);
              }
            }
          }
        }
        catch
        {
          ErrorMessage = string.Format(resString.ErrorCantReadFile, m_command_file_info[m_command_file_info.Length - 1].Name);
          success = false;
        }
      }
      else
      {
        // process parameters
        success = ProcessCommandLineArgument(in_command_line[i]);

        // next parameter
        i++;
      }
    }

    return success;
  }

  /// <summary>
  /// Processes one command line parameter
  /// </summary>
  /// <param name="in_argument"></param>
  /// <returns></returns>
  public bool ProcessCommandLineArgument(string in_argument)
  {
    string command;
    string parameter;
    string identifier;
		string[] options = null;
    string buffer;
    bool success = true;
    int i;
    bool inside_quote = false;

    // check for switch
    if (in_argument[0] == '-')
    {
      // split command line to command and parameter
      int semicolon_pos = in_argument.IndexOf(':');

      if (semicolon_pos == -1)
      {
        command = in_argument.Substring(1);
        parameter = "";
        identifier = "";
      }
      else
      {
        command = in_argument.Substring(1, semicolon_pos - 1);
        buffer = in_argument.Substring(semicolon_pos + 1, in_argument.Length - semicolon_pos - 1);

        // find coma
        i = 0;
        inside_quote = false;
        int coma_pos = -1;
        while (i < buffer.Length)
        {
          if (buffer[i] == '\"')
          {
            buffer = buffer.Remove(i, 1);
            inside_quote = !inside_quote;
          }
          else
          {
            if (!inside_quote && buffer[i] == ',')
            {
              coma_pos = i;
              break;
            }

            i++;
          }
        }

        // get identifier
        if (coma_pos == -1)
        {
          parameter = buffer;
          identifier = "";
        }
        else
        {
          parameter = buffer.Substring(0, coma_pos).Trim();

					// get options (if exists)
					int options_separator_pos;

					options_separator_pos = buffer.IndexOf(',', coma_pos + 1);

					if (options_separator_pos >= 0)
					{
						identifier = buffer.Substring(coma_pos + 1, options_separator_pos - coma_pos - 1).Trim();
						options = buffer.Substring(options_separator_pos + 1).Split(',');

						if (options != null)
						{
							for (int opt = 0; opt < options.Length; opt++)
							{
								options[opt] = options[opt].Trim();
							}
						}
					}
					else
					{
						// no parameter
						identifier = buffer.Substring(coma_pos + 1, buffer.Length - coma_pos - 1).Trim();
					}
        }

      }

      // store command
      Array.Resize(ref m_parameters, m_parameters.Length + 1);
      m_parameters[m_parameters.Length - 1] = new CommandLineParameters();
      m_parameters[m_parameters.Length - 1].Command = command.ToLower();
      m_parameters[m_parameters.Length - 1].Parameter = parameter;
      m_parameters[m_parameters.Length - 1].Identifier = identifier;
			m_parameters[m_parameters.Length - 1].Options = options;
    }
    else
    {
      // filename specified
      CommandFileInfo file_info = new CommandFileInfo();

      // check if already opened
      file_info.Name = in_argument;
      if (Array.IndexOf(m_command_file_info, file_info) != -1)
      {
        ErrorMessage = string.Format(resString.ErrorDuplicatedParameterFile, in_argument);
        success = false;
      }

      // open file
      if (success)
      {
        try
        {
          // open parameter file
          file_info.File = File.OpenText(file_info.Name);

          // store parameter file info
          Array.Resize(ref m_command_file_info, m_command_file_info.Length + 1);
          m_command_file_info[m_command_file_info.Length - 1] = file_info;

          // change current director to the parameter file's directory
          ChangeDirectory(file_info.Name);
        }
        catch
        {
          ErrorMessage = string.Format(resString.ErrorCantOpenParameterFile, in_argument);
          success = false;
        }
      }
    }
    
    return success;
  }

  private void ChangeDirectory(string in_file_name)
  {
    string dir = Path.GetDirectoryName(in_file_name);

    if(dir.Length>0)
      Directory.SetCurrentDirectory(dir);
  }

  /// <summary>
  /// Display help text if requested
  /// </summary>
  /// <returns></returns>
  public bool DisplayHelpIfRequested()
  {
    CommandLineParameters help = new CommandLineParameters();
    bool help_requested = false;

    // try 'help'
    help.Command = "help";
    if (Array.IndexOf(Parameters, help) != -1)
      help_requested = true;

    // try '?'
    help.Command = "?";
    if (Array.IndexOf(Parameters, help) != -1)
      help_requested = true;

    // if help requested
    if (help_requested)
    {
      Console.WriteLine(resString.Usage);
    }

    return help_requested;
  }
}
