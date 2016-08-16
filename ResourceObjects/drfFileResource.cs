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
// Base class for resource file objects
///////////////////////////////////////////////////////////////////////////////
using System;

public abstract class drfFileResource : drfBaseClass
{
  #region Data members
  protected string m_file_name;
  protected string m_resource_id;
  #endregion

  #region Constructor & Destructor
  public drfFileResource(UInt32 in_class_id, string in_class_name)
    : base(in_class_id,in_class_name)
  {
    m_file_name = "";
    m_resource_id = "";
  }
  #endregion

  #region Message processing
  protected override int ProcessMessage(drfmsgMessageBase in_message)
  {
    // process messages
    switch (in_message.MessageType)
    {
      // get identifier message
      case drfmsgMessageBase.drfmsgGetIdentifier:
        {
          drfmsgGetIdentifier message = (drfmsgGetIdentifier)in_message;

          message.Identifier = m_resource_id;
        }
        return 0;
    }
    
    return 0;
  }
  #endregion

  /// <summary>
  /// Cheks is file already loaded (exists)
  /// </summary>
  /// <param name="in_name"></param>
  /// <returns></returns>
  protected bool IsExists(string in_name)
  {
    // search for the class with same type and filename
    foreach (drfBaseClass cls in m_parent_class.Classes)
    {
      if (GetType() == cls.GetType())
      {
        drfFileResource file = (drfFileResource)cls;

        if (in_name == file.m_file_name)
          return true;
      }
    }

    return false;
  }
}

