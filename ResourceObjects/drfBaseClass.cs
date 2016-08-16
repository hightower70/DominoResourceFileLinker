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
// Base class for resource objects
///////////////////////////////////////////////////////////////////////////////
using System;

public abstract class drfBaseClass
{
  #region · Data members ·
  protected drfDominoResourceFile m_parent_class;
  protected clsBinaryBuffer m_binary_buffer;
  protected int m_file_pos;
  protected UInt32 m_class_id;
  protected string m_class_name;
  #endregion

  #region · Properties ·
  /// <summary>
  /// Binary buffer access
  /// </summary>
  public byte[] BinaryBuffer
  {
    get
    {
      return m_binary_buffer.Buffer;
    }
  }
  #endregion

  #region · Constructor ·
  public drfBaseClass(UInt32 in_class_id, string in_class_name)
  {
    m_class_id = in_class_id;
    m_class_name = in_class_name;
  }
  #endregion

  #region · Methods ·

  /// <summary>
  /// Sets parent class
  /// </summary>
  /// <param name="in_parent_class"></param>
  public void SetParentClass(drfDominoResourceFile in_parent_class)
  {
    m_parent_class = in_parent_class;
  }

  public UInt32 GetClassID()
  {
    return m_class_id;
  }

  public string GetClassName()
  {
    return m_class_name;
  }

  public int BaseProcessMessage(drfmsgMessageBase in_message)
  {
    int retval;

    switch (in_message.MessageType)
    {
      // Update file pos
      case drfmsgMessageBase.drfmsgPrepareBinaryData:
				retval = ProcessMessage(in_message);
				m_file_pos = ((drfmsgPrepareBinaryData)in_message).FilePos;
				((drfmsgPrepareBinaryData)in_message).FilePos += m_binary_buffer.Length;
        return retval;

      // get first entry file pos
      case drfmsgMessageBase.drfmsgGetFirstEntryFilePos:
        if (((drfmsgGetFirstEntryFilePos)in_message).FilePos == -1 && ((drfmsgGetFirstEntryFilePos)in_message).ClassId == m_class_id)
        {
          ((drfmsgGetFirstEntryFilePos)in_message).FilePos = m_file_pos;
        }
        return 0;

      default:
        return ProcessMessage(in_message);
    }
  }

  public int GetFilePosition()
  {
    return m_file_pos;
  }

  /// <summary>
  /// Sets error if resource identifier is already exists
  /// </summary>
  /// <param name="in_id">Identifier to check</param>
  /// <returns>True is success (identifier doesn't exists)</returns>
  public bool SetErrorIfResourceIdExists(string in_id)
  {
    drfmsgGetIdentifier message = new drfmsgGetIdentifier();

    // if identifier is empty
    if (in_id == null || in_id.Length == 0)
      return true;

    // check if identifier is exists
    foreach (drfBaseClass cls in m_parent_class.Classes)
    {
      message.Identifier = "";

      cls.BaseProcessMessage(message);

      if (message.Identifier == in_id)
      {
        m_parent_class.ErrorMessage = string.Format(resString.ErrorResourceIdentifierExsist, in_id);
        return false;
      }
    }

    return true;
  }

  public abstract int GetFilePositionPriority();
  protected abstract int ProcessMessage(drfmsgMessageBase in_message);
  #endregion
};

