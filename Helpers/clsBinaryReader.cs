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
// Binary file reader class
///////////////////////////////////////////////////////////////////////////////
using System;
using System.IO;

public class clsBinaryReader
{
  #region Data members
  private byte[] m_class_buffer;
  private int m_position;

  public int Position
  {
    set
    {
      m_position = value;
    }
    get
    {
      return m_position;
    }
  }

  #endregion

  #region Functions
  public bool Load(string in_name)
  {
    FileStream class_file;
    BinaryReader class_stream;
    long file_length;

    // load binary data of the class file
    class_file = File.Open(in_name, FileMode.Open);

    class_stream = new BinaryReader(class_file);
    file_length = class_file.Length;

    m_class_buffer = class_stream.ReadBytes((int)file_length);

    class_stream.Close();
    class_file.Close();

    m_position = 0;

    return true;
  }

  #endregion

  #region Data reader functions

  /// <summary>
  /// Gets byte from class file buffer (Big endian order)
  /// </summary>
  /// <param name="out_buffer">Data to get</param>
  public void ReadData(ref byte out_buffer)
  {
    out_buffer = m_class_buffer[m_position++];
  }

  /// <summary>
  /// Gets UInt32 from class file buffer (Big endian order)
  /// </summary>
  /// <param name="out_buffer">Data to get</param>
  public void ReadData(ref UInt32 out_buffer)
  {
    out_buffer = m_class_buffer[m_position] * 16777216u + m_class_buffer[m_position + 1] * 65536u + m_class_buffer[m_position + 2] * 256u + m_class_buffer[m_position + 3];
    m_position += 4;
  }

  /// <summary>
  /// Gets UInt16 from class file buffer (Big endian order)
  /// </summary>
  /// <param name="out_buffer"></param>
  public void ReadData(ref UInt16 out_buffer)
  {
    out_buffer = (UInt16)(m_class_buffer[m_position] * 256u + m_class_buffer[m_position + 1]);
    m_position += 2;
  }

  /// <summary>
  /// Gets Int32 from class file buffer (Big endian order)
  /// </summary>
  /// <param name="out_buffer">Data to get</param>
  public void ReadData(ref Int32 out_buffer)
  {
    byte[] buffer = new byte[4];

    buffer[3] = m_class_buffer[m_position++];
    buffer[2] = m_class_buffer[m_position++];
    buffer[1] = m_class_buffer[m_position++];
    buffer[0] = m_class_buffer[m_position++];

    out_buffer = BitConverter.ToInt32(buffer, 0);
  }

  /// <summary>
  /// Gets float from class file buffer (Big endian order)
  /// </summary>
  /// <param name="out_buffer">Data to get</param>
  public void ReadData(ref float out_buffer)
  {
    byte[] buffer = new byte[4];

    buffer[3] = m_class_buffer[m_position++];
    buffer[2] = m_class_buffer[m_position++];
    buffer[1] = m_class_buffer[m_position++];
    buffer[0] = m_class_buffer[m_position++];

    out_buffer = BitConverter.ToSingle(buffer, 0);
  }

  /// <summary>
  /// Gets Int64 from class file buffer (Big endian order)
  /// </summary>
  /// <param name="out_buffer">Data to get</param>
  public void ReadData(ref Int64 out_buffer)
  {
    byte[] buffer = new byte[8];
    int i;

    for (i = 7; i >= 0; i--)
      buffer[i] = m_class_buffer[m_position++];

    out_buffer = BitConverter.ToInt64(buffer, 0);

    m_position += 8;
  }

  /// <summary>
  /// Gets double from class file buffer (Big endian order)
  /// </summary>
  /// <param name="out_buffer">Data to get</param>
  public void ReadData(ref double out_buffer)
  {
    byte[] buffer = new byte[8];
    int i;

    for (i = 7; i >= 0; i--)
      buffer[i] = m_class_buffer[m_position++];

    out_buffer = BitConverter.ToDouble(buffer, 0);

    m_position += 8;
  }

  /// <summary>
  /// Gets raw byte data from file buffer.
  /// </summary>
  /// <param name="out_buffer">Data to get</param>
  public void ReadData(ref byte[] out_buffer)
  {
    for (int i = 0; i < out_buffer.Length; i++)
      out_buffer[i] = m_class_buffer[m_position++];
  }
  
#endregion
}