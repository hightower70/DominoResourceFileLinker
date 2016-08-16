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
// Java header class
///////////////////////////////////////////////////////////////////////////////
using System;
using System.Runtime.InteropServices;
using System.Diagnostics;

public class drfJavaHeader : drfBaseClass
{
  public const UInt32 ClassId = 0x534C434A;

#region Data members
  UInt16[] m_callback_function_table;
  #endregion

  /// <summary>
  /// Constructor
  /// </summary>
  public drfJavaHeader() : base(ClassId,"javaheader")
  {
  }

  #region Message handlers

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

      case drfmsgMessageBase.drfmsgUpdateBinaryData:
        return msgUpdateBinaryData((drfmsgUpdateBinaryData)in_message);
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
    // get callback function table size
    m_callback_function_table = new UInt16[m_parent_class.LinkerScript.GetMaxCallbackMethodIndex() + 1];

    // prepare binary data
    m_binary_buffer = new clsBinaryBuffer();

    m_binary_buffer.Length = sizeof(UInt16) +                                      // Callback function table pos
                             sizeof(UInt16) +                                      // Java classes pos
                             sizeof(UInt16) * m_callback_function_table.Length; ;  // callback table

    m_binary_buffer.Fill(0);

    return 0;
  }

  public void SetCallbackMethod(int in_index, int in_pos)
  {
    m_callback_function_table[in_index] = (UInt16)in_pos;
  }

  /// <summary>
  /// Updates binary data
  /// </summary>
  /// <param name="in_message"></param>
  /// <returns></returns>
  private int msgUpdateBinaryData(drfmsgUpdateBinaryData in_message)
  {
    int pos = 0;

    // update binary data
    pos = m_binary_buffer.Modify((UInt16)(2 * sizeof(UInt16)), 0);
    pos = m_binary_buffer.Modify((UInt16)(2 * sizeof(UInt16) + sizeof(UInt16) * m_callback_function_table.Length), pos);

    for (int i = 0; i < m_callback_function_table.Length; i++)
    {
      pos = m_binary_buffer.Modify(m_callback_function_table[i], pos);
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
    return 10;
  }
};
