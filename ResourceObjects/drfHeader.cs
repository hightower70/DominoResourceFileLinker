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
// resource file hader class
///////////////////////////////////////////////////////////////////////////////
using System;
using System.Runtime.InteropServices;

public class drfHeader : drfBaseClass
{
	#region · Constants ·
	const UInt16 MagicNumber = 0x5244;
  const UInt16 VersionNumber = 0x0001;
	#endregion

	#region · Types ·

	/// <summary>
	/// File header structure.
	/// </summary>
	struct FileHeader
  {
    public UInt16 m_magic_number;
    public UInt16 m_version;
    public UInt16 m_crc;
    public UInt32 m_size;
  };

  /// <summary>
  /// Chink info entry
  /// </summary>
  struct ChunkInfoEntry
  {
    public UInt32 ID;
    public drfBaseClass ClassEntry;

    public override bool Equals(object obj)
    {
      return ID == ((ChunkInfoEntry)obj).ID;
    }

    public override int GetHashCode()
    {
      return base.GetHashCode();
    }
  };
	#endregion

	#region · Data members ·
	ChunkInfoEntry[] m_entries;
  FileHeader m_header;
	#endregion

	#region · Message handlers ·
	/// <summary>
	/// Constructor
	/// </summary>
	public drfHeader() : base ( 0,"header" )
  {
    // init arrays
    m_entries = new ChunkInfoEntry[0];

    // fill in header
    m_header.m_magic_number = MagicNumber;
    m_header.m_version = VersionNumber;
  }

  /// <summary>
  /// Message handler
  /// </summary>
  /// <param name="in_message"></param>
  /// <returns></returns>
  protected override int ProcessMessage(drfmsgMessageBase in_message)
  {
    switch (in_message.MessageType)
    {
      case drfmsgMessageBase.drfmsgPrepareBinaryData:
        return msgPrepareBinaryData((drfmsgPrepareBinaryData)in_message);

      case drfmsgMessageBase.drfmsgUpdateCRCType:
        return msgUpdateCRC((drfmsgUpdateCRC)in_message);

      case drfmsgMessageBase.drfmsgUpdateBinaryData:
        return msgUpdateBinaryData((drfmsgUpdateBinaryData)in_message);

      case drfmsgMessageBase.drfmsgRegisterFileHeader:
        return msgRegisterFileHeader((drfmsgRegisterFileHeader)in_message);
    }

    return 0;
  }

  /// <summary>
  /// Prepares binary data.
  /// </summary>
  /// <param name="in_message"></param>
  /// <returns></returns>
  private int msgPrepareBinaryData(drfmsgPrepareBinaryData in_message)
  {
    // prepare binary data
    m_binary_buffer = new clsBinaryBuffer();
    
    m_binary_buffer.Length = Marshal.SizeOf(m_header) +                             // header size
                              m_entries.Length * (sizeof(UInt32) + sizeof(UInt32)); // chunk info size

    return 0;
  }

  /// <summary>
  /// Updates CRC
  /// </summary>
  /// <param name="in_message"></param>
  /// <returns></returns>
  private int msgUpdateCRC(drfmsgUpdateCRC in_message)
  {
    clsCITT16CRC crc = new clsCITT16CRC();
    int crc_pos = Marshal.SizeOf(m_header.m_magic_number) + Marshal.SizeOf(m_header.m_version);

    crc.AddToCRC(m_binary_buffer.Buffer, crc_pos + Marshal.SizeOf(m_header.m_crc), Marshal.SizeOf(m_header) - crc_pos - Marshal.SizeOf(m_header.m_crc));

    foreach(ChunkInfoEntry entry in m_entries)
      crc.AddToCRC(entry.ClassEntry.BinaryBuffer);

    m_header.m_crc = crc.CRC;

    crc.CRCBuffer.CopyTo(m_binary_buffer.Buffer, crc_pos);

    return 0;
  }

  /// <summary>
  /// Updates binary data
  /// </summary>
  /// <param name="in_message"></param>
  /// <returns></returns>
  private int msgUpdateBinaryData(drfmsgUpdateBinaryData in_message)
  {
    UInt16 word_buffer;
    UInt32 file_pos;
    int i;

    // calculate total size of the file
    m_header.m_size = 0;

    for (i = 0; i < m_parent_class.Classes.Length; i++)
      m_header.m_size += (UInt32)m_parent_class.Classes[i].BinaryBuffer.Length;

    // copy signature
    int pos = 0;

    pos = m_binary_buffer.Modify(m_header.m_magic_number, pos);

    // copy version
    pos = m_binary_buffer.Modify(m_header.m_version, pos);

    // copy crc
    pos = m_binary_buffer.Modify(m_header.m_crc, pos);

    // copy size
    pos = m_binary_buffer.Modify(m_header.m_size, pos);

    // copy table element count
    word_buffer = (UInt16)m_entries.Length;
    pos = m_binary_buffer.Modify(word_buffer, pos);

	  // update table addresses and save table
    for( i = 0; i < m_entries.Length; i++ )
    {
      // get entry position
      file_pos = (UInt32)(m_entries[i].ClassEntry.GetFilePosition());

      // save table entry
      pos = m_binary_buffer.Modify(m_entries[i].ID, pos);
      pos = m_binary_buffer.Modify(file_pos, pos);
	  }

    return 0;
  }

  /// <summary>
  /// Resgister class into the file header
  /// </summary>
  /// <param name="in_messsage"></param>
  /// <returns></returns>
  private int msgRegisterFileHeader(drfmsgRegisterFileHeader in_message)
  {
    int i;
    int insert;

    for (i = 0; i < m_entries.Length; i++)
    {
      // if this class is already registered do nothing
      if (m_entries[i].ID == in_message.Id)
        break;
    }

    // if not registered insert into the list
    if (i == m_entries.Length)
    {
      // find insertion point
      i=0;
      while (i < m_entries.Length && m_entries[i].ClassEntry.GetFilePositionPriority() <= in_message.Entry.GetFilePositionPriority() )
        i++;

      // add item storage
      Array.Resize(ref m_entries, m_entries.Length + 1);

      // shift items back
      insert = i;
      i = m_entries.Length - 1;
      while (i > insert )
      {
        m_entries[i]=m_entries[i-1];
        i--;
      }

      // insert item
      m_entries[insert].ID = in_message.Id;
      m_entries[insert].ClassEntry = in_message.Entry;
    }

    return 0;
  }

  #endregion

  /// <summary>
  /// Returns file position priority index.
  /// </summary>
  /// <returns></returns>
  public override int GetFilePositionPriority()
  {
    return 0;
  }
};
