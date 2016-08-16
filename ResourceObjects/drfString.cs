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
// String resource object handler
///////////////////////////////////////////////////////////////////////////////
using System;
using System.Text;

public class drfString : drfFileResource
{
	#region · Constants ·
	public const UInt32 ClassId = 0x47525453;
	#endregion

	#region · Data Members ·
	string m_string;
	#endregion

	#region · Constructor&Destructor ·
	/// <summary>
	/// Constructor
	/// </summary>
	public drfString() : base(ClassId,"string")
  {
    m_string = "";
  }

	#endregion

	#region · Message handlers ·
	/// <summary>
	/// Message handler
	/// </summary>
	/// <param name="in_message"></param>
	/// <returns></returns>
	protected override int ProcessMessage(drfmsgMessageBase in_message)
  {
		int retval;

		retval = base.ProcessMessage(in_message);

		switch (in_message.MessageType)
    {
      case drfmsgMessageBase.drfmsgProcessCommandLine:
        return msgProcessCommandLine((drfmsgProcessCommandLine)in_message);

      case drfmsgMessageBase.drfmsgPrepareBinaryData:
        return msgPrepareBinaryData((drfmsgPrepareBinaryData)in_message);

      case drfmsgMessageBase.drfmsgUpdateBinaryData:
        return msgUpdateBinaryData((drfmsgUpdateBinaryData)in_message);

      case drfmsgMessageBase.drfmsgDisplayHelpMessage:
        return msgDisplayHelpMessage((drfmsgDisplayHelpMessage)in_message);
    }

    return 0;
  }

  /// <summary>
  /// Updates binary data
  /// </summary>
  /// <param name="in_message"></param>
  /// <returns></returns>
  private int msgUpdateBinaryData(drfmsgUpdateBinaryData in_message)
  {
    // update binary data
    int pos = 0;

    UTF8Encoding utf8 = new UTF8Encoding();

    byte[] encoded_bytes = utf8.GetBytes(m_string);
    byte[] length_buffer;

    EncodeLength(out length_buffer, m_string.Length);

    pos = m_binary_buffer.Modify(length_buffer, pos);
    pos = m_binary_buffer.Modify(encoded_bytes, pos);

    return 0;
  }

  /// <summary>
  /// Prepare binary data
  /// </summary>
  /// <param name="in_message"></param>
  /// <returns></returns>
  private int msgPrepareBinaryData(drfmsgPrepareBinaryData in_message)
  {
    // prepare binary data
    m_binary_buffer = new clsBinaryBuffer();

    int length = m_string.Length;
    byte[] buffer;

    EncodeLength(out buffer, length);

    m_binary_buffer.Length = length + // string storage size
                             buffer.Length; // length storage size
 
    return 0;
  }

  /// <summary>
  /// Processes command line switches
  /// </summary>
  /// <param name="in_message"></param>
  /// <returns></returns>
  private int msgProcessCommandLine(drfmsgProcessCommandLine in_message)
  {
    if (in_message.Command == "string")
    {
      // check identifier
      if (SetErrorIfResourceIdExists(in_message.Identifier))
      {
        // add string class
        drfString str = new drfString();
        m_parent_class.AddClass(str);

        // set string
        str.SetString(in_message.Parameter);

        // set resource information
        str.m_resource_id = in_message.Identifier;

        // register font class in the header
        drfmsgRegisterFileHeader register_message = new drfmsgRegisterFileHeader();

        register_message.Entry = str;
        register_message.Id = ClassId;

        m_parent_class.BroadcastMessage(register_message);
      }

      in_message.Used = true;
    }

    return 0;
  }

  /// <summary>
  /// Displays help message
  /// </summary>
  /// <param name="in_message"></param>
  /// <returns></returns>
  private int msgDisplayHelpMessage(drfmsgDisplayHelpMessage in_message)
  {
    Console.WriteLine(resString.UsageString);

    return 0;
  }

	#endregion

	#region · Member functions ·

	/// <summary>
	/// Returns file position priority index.
	/// </summary>
	/// <returns></returns>
	public override int GetFilePositionPriority()
  {
    return 40;
  }

  public void SetString(string in_string)
  {
    m_string = in_string;
  }

  public void EncodeLength(out byte[] out_buffer, int in_length)
  {
    // init
    int length = in_length;
    out_buffer = new byte[0];

    // store high bytes
    while (length > 127)
    {
      Array.Resize(ref out_buffer, out_buffer.Length + 1);
      out_buffer[out_buffer.Length - 1] = (byte)((length & 0x7f) | 0x80);
      length >>= 7;
    }

    // store low byte
    Array.Resize(ref out_buffer, out_buffer.Length + 1);
    out_buffer[out_buffer.Length - 1] = (byte)((length & 0x7f));
  }

  #endregion
}
