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
// Resource file class
///////////////////////////////////////////////////////////////////////////////
using System;
using System.IO;

public class drfDominoResourceFile
{
  #region · Data members ·
  private drfBaseClass[] m_classes;
  private drfBaseClass[] m_factory_classes;
  private string m_error_message;
  private drfLinkerScript m_linker_script;
  private bool m_verbose_messages = false;
  #endregion

  #region · Constructor ·

  /// <summary>
  /// Constructor. Adds header to file.
  /// </summary>
  public drfDominoResourceFile()
  {
    m_classes = new drfBaseClass[0];
    m_factory_classes = new drfBaseClass[0];

    // create header
    AddClass(new drfHeader());

    // create liker script
    m_linker_script = new drfLinkerScript();
    AddFactoryClass(m_linker_script);
  }

  #endregion

  #region · Properties ·

  /// <summary>
  /// Gets list of classes included in the file
  /// </summary>
  public drfBaseClass[] Classes
  {
    get
    {
      return m_classes;
    }
  }

  /// <summary>
  /// Gets linker script
  /// </summary>
  public drfLinkerScript LinkerScript
  {
    get
    {
      return m_linker_script;
    }
  }

  /// <summary>
  /// Get verbose messages enabled flag
  /// </summary>
  public bool VerboseMessages
  {
    get
    {
      return m_verbose_messages;
    }
  }

  /// <summary>
  /// True if error occured (error message is not empty)
  /// </summary>
  /// <returns></returns>
  public bool IsError()
  {
    return m_error_message != null && m_error_message.Length != 0;
  }

  /// <summary>
  /// Sets/gets error message
  /// </summary>
  public string ErrorMessage
  {
    get
    {
      return m_error_message;
    }

    set
    {
      m_error_message = value;
    }
  }

  #endregion


  /// <summary>
  /// Adds new class to the resource file.
  /// </summary>
  /// <param name="in_class"></param>
  public void AddClass(drfBaseClass in_class)
  {
    // set parent class
    in_class.SetParentClass(this);

    // search class with lower file postion priority
    int class_index = 0;

    while (class_index < m_classes.Length && m_classes[class_index].GetFilePositionPriority() <= in_class.GetFilePositionPriority())
      class_index++;

    // increase size of the array
    Array.Resize(ref m_classes, m_classes.Length + 1);

    // move remaining classes one backward
    for (int i = m_classes.Length-1; i > class_index; i--)
      m_classes[i] = m_classes[i-1];

    // insert this entry to the specified position
    m_classes[class_index] = in_class;
  }

  /// <summary>
  /// Adds a new factory class
  /// </summary>
  /// <param name="in_class"></param>
  public void AddFactoryClass(drfBaseClass in_class)
  {
    // set parent class
    in_class.SetParentClass(this);

    // increase size of the array
    Array.Resize(ref m_factory_classes, m_factory_classes.Length + 1);

    // append this entry to the end of the list
    m_factory_classes[m_factory_classes.Length - 1] = in_class;
  }

  /// <summary>
  /// Broadcasts message.
  /// </summary>
  /// <param name="in_message"></param>
  /// <returns></returns>
  public void BroadcastMessage(drfmsgMessageBase in_message)
  {
    int i;

    for (i = 0; i < m_classes.Length; i++ )
      m_classes[i].BaseProcessMessage(in_message);
  }

  /// <summary>
  /// Broadcasts factory message.
  /// </summary>
  /// <param name="in_message"></param>
  /// <returns></returns>
  public void BroadcastFactoryMessage(drfmsgMessageBase in_message)
  {
    foreach (drfBaseClass cls in m_factory_classes)
      cls.BaseProcessMessage(in_message);
  }


  /// <summary>
  /// Returns the file binary size
  /// </summary>
  /// <returns></returns>
  public int GetBinarySize()
  {
    int i;
    int size = 0;

    for (i = 0; i < m_classes.Length; i++)
      size += m_classes[i].BinaryBuffer.Length;

    return size;
  }

  /// <summary>
  /// Sets verbose messages flag (from the command line switch)
  /// </summary>
  /// <param name="in_verbose_messages"></param>
  public void SetVerboseMessageFlag(bool in_verbose_messages)
  {
    m_verbose_messages = in_verbose_messages;
  }

  #region · Save functions ·

  /// <summary>
  /// Saves resource file
  /// </summary>
  public void Save()
  {
    string file_format = LinkerScript.OutputFileFormat;
    bool saved = false;

    if (file_format == "HEX" && LinkerScript.OutputFileName != "")
    {
      SaveToIntelHex(LinkerScript.OutputFileName);
      saved = true;
    }
    
    if (file_format == "BIN" && LinkerScript.OutputFileName != "")
    {
      SaveToBinary(LinkerScript.OutputFileName);
      saved = true;
    }

    if (file_format == "C" && LinkerScript.OutputFileFormat != "")
    {
      SaveToC(LinkerScript.OutputFileName);
      saved = true;
    }

    if (!saved)
    {
      m_error_message = resString.ErrorFileFormatOrNameNotSpecified;
    }
  }

  /// <summary>
  /// Save to binary 
  /// </summary>
  /// <returns></returns>
  private void SaveToBinary(string in_name)
  {
    // create file
    FileStream stream = new FileStream(in_name, FileMode.Create);
    BinaryWriter file = new BinaryWriter(stream);

    // go thrugh all entries and save them
    for (int i = 0; i < m_classes.Length; i++)
      if (m_classes[i].BinaryBuffer.Length > 0)
        file.Write(m_classes[i].BinaryBuffer);

    // close file
    file.Close();
    stream.Close();
  }

  private void SaveToC(string in_name)
  {
    string buffer = "";
    int byte_count = 0;

    // create file
    StreamWriter file = File.CreateText(in_name);

    //  write first line
    file.WriteLine(string.Format("{0} {1}[{2}] =", LinkerScript.GetLinkerSettings("type"), LinkerScript.GetLinkerSettings("variable"), GetBinarySize()));
    file.WriteLine("{");

    for (int i = 0; i < m_classes.Length; i++)
    {
      for (int j = 0; j < m_classes[i].BinaryBuffer.Length; j++)
      {
        if (buffer == "")
          buffer = " 0x";
        else
          buffer += ", 0x";
        
        buffer += m_classes[i].BinaryBuffer[j].ToString("X2");
        byte_count++;

        if (byte_count == 8)
        {
          byte_count = 0;
          file.WriteLine(buffer + ',');
          buffer = "";
        }
      }
    }

    // write last line
    if (buffer != "")
      file.WriteLine(buffer);

    file.WriteLine("};");

    // close file
    file.Close();
  }

  private void SaveToIntelHex(string in_name)
  {/*
  	drcRESEntry* entry;
    bool success = true;
    fclWord active_segment_address = 0;
    fclDWord address = 0;
  	fclTextFile file;
    fclWord checksum;
    fclString line_buffer;
    fclByteBuffer* binary_buffer;
    int binary_buffer_position;
    fclInt byte_counter;
    fclString string_buffer;
    */

    // get start address
    int byte_counter = 0;
    UInt32 address = 0;
    UInt16 active_segment_address = 0;
    int entry_index = 0;
    byte checksum = 0;
    string line_buffer;
    string string_buffer;
    string start_address = LinkerScript.GetLinkerSettings("startaddress").Trim().ToLower();

    if (start_address.StartsWith("0x"))
      start_address = start_address.Remove(0, 2);

    UInt32.TryParse(start_address, System.Globalization.NumberStyles.HexNumber, null, out address);

    // create file
    StreamWriter file = new StreamWriter(in_name);

    // go through all entries
    entry_index = 0;
    while (entry_index < m_classes.Length)
    {
      // write segment address if necessary
      if ( (UInt16)(address >> 16) != active_segment_address)
      {
        // change segment address
        active_segment_address = (UInt16)(address >> 16);

        // create record
        checksum = 0;
        line_buffer = ":";

        // reclen
        line_buffer += "02";
        checksum += 0x02;

        // offset
        line_buffer += "0000";
        checksum += 0x00;
        checksum += 0x00;

        // rectype
        line_buffer += "02";
        checksum += 0x02;

        // segment address
        line_buffer += ((UInt16)(active_segment_address << 12)).ToString("X4");
        checksum += (byte)((active_segment_address << 12) & 0xff);
        checksum += (byte)((active_segment_address << 12) >> 8);

        // checksum
        line_buffer += ((byte)(0x0100 - checksum)).ToString("X2");

        file.WriteLine(line_buffer);
      }

      // send 16 bytes
      byte_counter = 0;
      line_buffer = ":00";
      checksum = 0;

      // offset
      line_buffer += address.ToString("X4");
      checksum += (byte)(address & 0xff);
      checksum += (byte)((address >> 8) & 0xff);

      // rectype
      line_buffer += "00";
      checksum += 0x00;

      /*
      // data
      while (byte_counter < 16 && entry != NULL && success)
      {
        // go to the next object if necessary
        while (entry != NULL && binary_buffer_position >= binary_buffer->GetSize())
        {
          binary_buffer_position = 0;
          entry = entry->m_next_entry;

          if (entry != NULL)
            binary_buffer = &(entry->m_binary);
        }

        // write one byte
        if (entry != NULL)
        {
          AddHexDigit(line_buffer, 2, binary_buffer->GetData(binary_buffer_position));
          AddToChecksum(checksum, binary_buffer->GetData(binary_buffer_position));
          address++;
          binary_buffer_position++;
          byte_counter++;
        }
      }
      */
      // update record length
      string_buffer = "";
      string_buffer = byte_counter.ToString("X2");
      line_buffer = line_buffer.Remove(1,2);
      line_buffer = line_buffer.Insert(1,string_buffer);
      checksum += (byte)byte_counter;

      // write checksum
      line_buffer += ((byte)(0x0100 - checksum)).ToString("X2");
      
      // write line
      if (byte_counter > 0)
        file.WriteLine(line_buffer);
    }

    // write last line
    file.WriteLine(":00000001FF");

    // close file
    file.Close();



/*
      // create file
	success = file.Open( m_file_name, FCLFF_Create );

	// go thrugh all entries and save them
	entry = m_entries;
  binary_buffer_position = 0;
  binary_buffer = &(entry->m_binary);
	while( entry != NULL && success )
	{
    // write segment address if necessary
    if( (address >> 16) != active_segment_address )
    {
      // change segment address
      active_segment_address = (address>>16);

      // create record
      checksum = 0;
      line_buffer = ":";

      // reclen
      AddHexDigit( line_buffer, 2, 0x02 );
      AddToChecksum( checksum, 0x02 );

      // offset
      AddHexDigit( line_buffer, 4, 0x0000 );
      AddToChecksum( checksum, 0x00 );
      AddToChecksum( checksum, 0x00 );

      // rectype
      AddHexDigit( line_buffer, 2, 0x02 );
      AddToChecksum( checksum, 0x02 );

      // segment address
      AddHexDigit( line_buffer, 4, active_segment_address << 12 );
      AddToChecksum( checksum, (active_segment_address << 12 ) & 0xff );
      AddToChecksum( checksum, (active_segment_address << 12 ) >> 8 );

      // checksum
      AddHexDigit( line_buffer, 2, 0x0100 - checksum );

      file.WriteLine( line_buffer );
    }

    // send 16 bytes
    byte_counter = 0;
    line_buffer = ":00";
    checksum = 0;

    // offset
    AddHexDigit( line_buffer, 4, address );
    AddToChecksum( checksum, address & 0xff );
    AddToChecksum( checksum, (address>>8)&0xff );

    // rectype
    AddHexDigit( line_buffer, 2, 0x00 );
    AddToChecksum( checksum, 0x00 );

    // data
    while( byte_counter < 16 && entry != NULL && success )
    {
      // go to the next object if necessary
      while( entry != NULL && binary_buffer_position >= binary_buffer->GetSize() )
      {
        binary_buffer_position = 0;
        entry = entry->m_next_entry;

        if( entry != NULL )
          binary_buffer =  &(entry->m_binary);
      }

      // write one byte
      if( entry != NULL )
      {
        AddHexDigit( line_buffer, 2, binary_buffer->GetData( binary_buffer_position ) );
        AddToChecksum( checksum, binary_buffer->GetData( binary_buffer_position ) );
        address++;
        binary_buffer_position++;
        byte_counter++;
      }
    }

    // update record length
    string_buffer = "";
    AddHexDigit( string_buffer, 2, byte_counter );
    line_buffer[1] = string_buffer[0];
    line_buffer[2] = string_buffer[1];
    AddToChecksum( checksum, byte_counter );

    // write checksum
    AddHexDigit( line_buffer, 2, 0x0100 - checksum );

    // write line
    if( byte_counter > 0 )
      file.WriteLine( line_buffer );

	}

  // write last line
  file.WriteLine(":00000001FF");

	// close file
	file.Close();*/
  }

  #endregion
};