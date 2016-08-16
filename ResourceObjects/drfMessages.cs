///////////////////////////////////////////////////////////////////////////////
// Copyright (c) 2005-2014 Laszlo Arvai. All rights reserved.
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
// Parser messages classes
///////////////////////////////////////////////////////////////////////////////
using System;

/// <summary>
/// Base class for all messages
/// </summary>
public abstract class drfmsgMessageBase
{
  // message types
  #region Message type constants
  // normal messages
  public const int drfmsgPrepareBinaryData = 1;
  public const int drfmsgLoadBinaryData = 2;
  public const int drfmsgUpdateCRCType = 3;
  public const int drfmsgUpdateBinaryData = 4;
  public const int drfmsgJavaFindMethod = 5;
  public const int drfmsgJavaFindClass = 6;
  public const int drfmsgRegisterFileHeader = 7;
  public const int drfmsgLinkEntries = 8;
  public const int drfmsgGetJavaMethodChunkPos = 9;
  public const int drfmsgGetFirstEntryFilePos = 10;
  public const int drfmsgUpdateFilePosType = 11;
  public const int drfmsgGetIdentifier = 12;

  // factory messages
  public const int drfmsgDisplayHelpMessage = 1000;
  public const int drfmsgProcessCommandLine = 1001;

  #endregion

  private int m_message_type;

  protected drfmsgMessageBase(int in_type)
  {
    m_message_type = in_type;
  }

  public int MessageType
  {
    get
    {
      return m_message_type;
    }
  }
};

/// <summary>
/// Prepare binary data message
/// </summary>
public class drfmsgPrepareBinaryData : drfmsgMessageBase
{
	public int FilePos; // position of the class within the binary resource file

	public drfmsgPrepareBinaryData() : base(drfmsgPrepareBinaryData)
  {
		FilePos = 0;
	}

	public void AddLength(int in_length)
	{
		FilePos += in_length;
	}

};

/// <summary>
/// Prepare binary data message
/// </summary>
public class drfmsgLoadBinaryData : drfmsgMessageBase
{
  public drfmsgLoadBinaryData()
    : base(drfmsgLoadBinaryData)
  {
  }
};

/// <summary>
/// Update CRC message
/// </summary>
public class drfmsgUpdateCRC : drfmsgMessageBase
{
  public drfmsgUpdateCRC()
    : base(drfmsgUpdateCRCType)
  {

  }
};

/// <summary>
/// Update Binary data
/// </summary>
public class drfmsgUpdateBinaryData : drfmsgMessageBase
{
  public drfmsgUpdateBinaryData()
    : base(drfmsgUpdateBinaryData)
  {

  }
};

/// <summary>
/// Display help message 
/// </summary>
public class drfmsgDisplayHelpMessage : drfmsgMessageBase
{
  public drfmsgDisplayHelpMessage()
    : base(drfmsgDisplayHelpMessage)
  {

  }
};

/// <summary>
/// Process command line parameters message
/// </summary>
public class drfmsgProcessCommandLine : drfmsgMessageBase
{
  public string Command;
  public string Parameter;
  public string Identifier;
  public bool Used;
	public string[] Options;

  public drfmsgProcessCommandLine(ref sysCommandLine.CommandLineParameters in_command_line)
    : base(drfmsgProcessCommandLine)
  {
    Command = in_command_line.Command;
    Parameter = in_command_line.Parameter;
    Identifier = in_command_line.Identifier;
    Used = in_command_line.Used;
		Options = in_command_line.Options;
  }

};

/// <summary>
/// Find Java Method
/// </summary>
public class drfmsgJavaFindMethod : drfmsgMessageBase
{
  public string ClassName;
  public string MethodName;
  public drfJavaClass ClassEntry;
  public int MethodIndex;

  public drfmsgJavaFindMethod()
    : base(drfmsgJavaFindMethod)
  {
    ClassName = "";
    MethodName = "";
    ClassEntry = null;
    MethodIndex = -1;
  }
};

/// <summary>
/// Returns the specified Java class
/// </summary>
public class drfmsgJavaFindClass : drfmsgMessageBase
{
  public string ClassName;
  public drfJavaClass ClassEntry;

  public drfmsgJavaFindClass()
    : base(drfmsgJavaFindClass)
  {
    ClassName = "";
    ClassEntry = null;
  }
};

/// <summary>
/// Registers the specifid class into the resource file header
/// </summary>
public class drfmsgRegisterFileHeader : drfmsgMessageBase
{
  public uint Id;
  public drfBaseClass Entry;

  public drfmsgRegisterFileHeader()
    : base(drfmsgRegisterFileHeader)
  {
  }

};

/// <summary>
/// Registers the specifid class into the resource file header
/// </summary>
public class drfmsgGetIdentifier : drfmsgMessageBase
{
  public string Identifier;
 
  public drfmsgGetIdentifier()
    : base(drfmsgGetIdentifier)
  {
  }

};

/// <summary>
/// Link entries
/// </summary>
public class drfmsgLinkEntries : drfmsgMessageBase
{
  public drfmsgLinkEntries()
    : base(drfmsgLinkEntries)
  {
  }
}


public class drfmsgGetFirstEntryFilePos : drfmsgMessageBase
{
  public UInt32 ClassId;
  public int FilePos;

  public drfmsgGetFirstEntryFilePos()
    : base(drfmsgGetFirstEntryFilePos)
  {
    FilePos = -1;
  }
}

public class drfmsgGetJavaMethodChunkPos : drfmsgMessageBase
{
  public drfJavaClass ClassEntry;
  public int MethodIndex;
  public int MethodPos;

  public drfmsgGetJavaMethodChunkPos()
    : base(drfmsgGetJavaMethodChunkPos)
  {
  }
}