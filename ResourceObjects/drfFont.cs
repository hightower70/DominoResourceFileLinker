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
// Font resource object handler
///////////////////////////////////////////////////////////////////////////////
using System;
using System.IO;
using System.Runtime.InteropServices;

public class drfFont : drfFileResource
{
  #region · Constants ·

  public const UInt32 ClassId = 0x544e4f46;

	const int fntFT_MONOCHROME = 0;

  // fnt flags
  const int fntFF_DONTCARE    = (0<<4);
  const int fntFF_ROMAN       = (1<<4);
  const int fntFF_SWISS	  		= (2<<4);  
  const int fntFF_MODERN  		=	(3<<4);
  const int fntFF_SCRIPT  		=	(4<<4);
  const int fntFF_DECORATIVE  = (5<<4);
  const int fntFF_MASK        =	0xf0;

  // Font flags
  const int FF_DONTCARE   =	1;				// Don't care or don't know.
  const int FF_ROMAN      =	2;				// Proportionally spaced fonts with serifs.
  const int FF_SWISS      =	3;				// Proportionally spaced fonts without serifs.
  const int FF_MODERN     =	4;				// Fixed-pitch fonts.
  const int FF_SCRIPT     =	5;				// Script
  const int FF_DECORATIVE	=	6;				// Decorative
  const int FF_FIXED      =	(1<<7); 	// Set if the font is fixed
  const int FF_BOLD				=	(1<<6);   // Set if the font is bold
  const int FF_ITALIC     =	(1<<5); 	// Set if the font is italic
  const int FF_INVALID    = 0;				// invalid family type
  #endregion
    
  #region · Types ·
  /// <summary>
  /// Character information class
  /// </summary>
  public class CharInfo : IComparable
  {
	  public int code;
	  public int width;
	  public byte[] bitmap;

    public int CompareTo(object obj)
    {
      if (obj is CharInfo)
      {
        CharInfo otherCharInfo = (CharInfo)obj;
        return this.code.CompareTo(otherCharInfo.code);
      }
      else
      {
        throw new ArgumentException("Object is not a CharInfo");
      }
    }
  }

  [StructLayout(LayoutKind.Explicit, Size=118)]
  public struct FNTFileHeader
  { 
    [FieldOffset(0)] public UInt16 dfVersion; 
    [FieldOffset(2)] public UInt32 dfSize; 
    [FieldOffset(6)] public byte Copyright;
    [FieldOffset(66)] public UInt16 dfType;
    [FieldOffset(68)] public UInt16 dfPoints;
    [FieldOffset(70)] public UInt16 dfVertRes;
    [FieldOffset(72)] public UInt16 dfHorizRes;
    [FieldOffset(74)] public UInt16 dfAscent;
    [FieldOffset(76)] public UInt16 dfInternalLeading;
    [FieldOffset(78)] public UInt16 dfExternalLeading;
    [FieldOffset(80)] public byte dfItalic;
    [FieldOffset(81)] public byte dfUnderline;
    [FieldOffset(82)] public byte dfStrikeOut;
    [FieldOffset(83)] public UInt16 dfWeight;
    [FieldOffset(85)] public byte dfCharSet;
    [FieldOffset(86)] public UInt16 dfPixWidth;
    [FieldOffset(88)] public UInt16 dfPixHeight;
    [FieldOffset(90)] public byte dfPitchAndFamily;
    [FieldOffset(91)] public UInt16 dfAvgWidth;
    [FieldOffset(93)] public UInt16 dfMaxWidth;
    [FieldOffset(95)] public byte dfFirstChar;
    [FieldOffset(96)] public byte dfLastChar;
    [FieldOffset(97)] public byte dfDefaultChar;
    [FieldOffset(98)] public byte dfBreakChar;
    [FieldOffset(99)] public UInt16 dfWidthBytes;
    [FieldOffset(101)] public UInt32 dfDevice;
    [FieldOffset(105)] public UInt32 dfFace;
    [FieldOffset(109)] public UInt32 dfBitsPointer;
    [FieldOffset(113)] public UInt32 dfBitsOffset;
    [FieldOffset(117)] public byte dfReserved;
  }

  #endregion

  #region · Data Members ·
  string m_name;
  byte m_flags;
	byte m_type;
	int m_horizontal_char_gap;
  int m_height;
  int m_width;
  int m_baseline;
  int m_default_character;
  int m_binary_size;
  CharInfo[] m_characters;
  int m_line_number;
  #endregion

  #region · Constructor&Destructor ·
  /// <summary>
  /// Constructor
  /// </summary>
  public drfFont() : base(ClassId,"font")
  {
    m_file_name = "";
    m_flags = 0;
    m_height = 0;
    m_width = 0;
    m_baseline = 0;
    m_binary_size = 0;
    m_line_number = -1;
		m_type = 0;
		m_horizontal_char_gap = 0;
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

    return retval;
  }

  /// <summary>
  /// Updates binary data
  /// </summary>
  /// <param name="in_message"></param>
  /// <returns></returns>
  private int msgUpdateBinaryData(drfmsgUpdateBinaryData in_message)
  {
    int pos;
    int char_data_pos = 0;
    int minascii = -1;
    int maxascii = -1;
    int i;
    int unicode_char_count = 0;

    // cache data
    for (i = 0; i < m_characters.Length; i++)
    {
      // store ansii information
      if (m_characters[i].code <= 0xff)
      {
        if (minascii == -1)
          minascii = m_characters[i].code;

        maxascii = m_characters[i].code;
      }
      else
        unicode_char_count++;
    }

    // init
    pos = 0;

    // store header
		pos = m_binary_buffer.Modify((byte)m_type, pos);                // type
    pos = m_binary_buffer.Modify((byte)m_flags, pos);               // flags
    pos = m_binary_buffer.Modify((byte)m_width, pos);               // width
    pos = m_binary_buffer.Modify((byte)m_height, pos);              // height
    pos = m_binary_buffer.Modify((byte)m_baseline, pos);            // baseline
		pos = m_binary_buffer.Modify((byte)m_horizontal_char_gap, pos); // horizontal character gap
		pos = m_binary_buffer.Modify((byte)minascii, pos);              // minascii
    pos = m_binary_buffer.Modify((byte)maxascii, pos);              // maxascii
    pos = m_binary_buffer.Modify((byte)m_default_character, pos );  // default character
    pos = m_binary_buffer.Modify((UInt16)unicode_char_count, pos);  // unicode char count

    // ascii table
    char_data_pos = pos +
                    (maxascii - minascii + 1) * sizeof(UInt16) + // ascii table length
                    (unicode_char_count * (sizeof(UInt16) + sizeof(UInt16)));
    
    for (i = 0; i < m_characters.Length; i++)
    {
      // store entry in the table (address)
			pos = m_binary_buffer.Modify((UInt16)(char_data_pos), pos);

      // store width
      char_data_pos = m_binary_buffer.Modify((byte)m_characters[i].width,char_data_pos);

      // store bitmap
      char_data_pos = m_binary_buffer.Modify(m_characters[i].bitmap, char_data_pos);
    }

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
    m_binary_buffer.Length = m_binary_size;
 
    return 0;
  }

  /// <summary>
  /// Processes command line switches
  /// </summary>
  /// <param name="in_message"></param>
  /// <returns></returns>
  private int msgProcessCommandLine(drfmsgProcessCommandLine in_message)
  {
    if (in_message.Command == "font")
    {
      if (!IsExists(in_message.Parameter))
      {
        // check identifier
        if (SetErrorIfResourceIdExists(in_message.Identifier))
        {
          // add font file
          drfFont font_file = new drfFont();
          m_parent_class.AddClass(font_file);

          if (font_file.Load(in_message.Parameter))
          {
            // set resource information
            font_file.m_file_name = in_message.Parameter;
            font_file.m_resource_id = in_message.Identifier;

            // register font class in the header
            drfmsgRegisterFileHeader register_message = new drfmsgRegisterFileHeader();

            register_message.Entry = font_file;
            register_message.Id = ClassId;

            m_parent_class.BroadcastMessage(register_message);
          }
          else
          {
						if( font_file.m_line_number > 0 )
							m_parent_class.ErrorMessage = string.Format(resString.ErrorInvalidFNAFile, in_message.Parameter, font_file.m_line_number);
						else
							m_parent_class.ErrorMessage = string.Format(resString.ErrorCantOpenResourceFile, in_message.Parameter);
          }
        }

        in_message.Used = true;
      }
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
    Console.WriteLine(resString.UsageFont);

    return 0;
  }

  #endregion

  #region · Member functions ·

  /// <summary>
  /// Read structure from file
  /// </summary>
  /// <typeparam name="T"></typeparam>
  /// <param name="fs"></param>
  /// <returns></returns>
  private T ReadStruct<T>(FileStream fs)
  {
    byte[] buffer = new byte[Marshal.SizeOf(typeof(T))];
    fs.Read(buffer, 0, Marshal.SizeOf(typeof(T)));
    GCHandle handle = GCHandle.Alloc(buffer, GCHandleType.Pinned);
    T temp = (T)Marshal.PtrToStructure(handle.AddrOfPinnedObject(), typeof(T));
    handle.Free();

    return temp;
  }

  /// <summary>
  /// Load font file
  /// </summary>
  /// <param name="in_filename"></param>
  /// <returns></returns>
  bool Load( string in_filename )
  {
	  string ext;
	  bool success = false;
    int i;

    // init
	  m_name = "";
	  m_flags			= FF_INVALID;
	  m_height		= -1;
	  m_width			= -1;
	  m_baseline	= -1;

	  m_characters = new CharInfo[0];

    // store name
    m_file_name = in_filename;

    // get extension
    ext = Path.GetExtension( in_filename ).ToUpper();

	  // Load FNA files
	  if( ext == ".FNA" )
	  {
	  	success = LoadFNA( in_filename );
	  }
	  else
	  {
	  	if( ext == ".FNT" )
		  {
		    success = LoadFNT( in_filename );
		  }
		  else
		  	success = false;
	  }

    // if success update binary size
    if( success )
    {
      // header size
			m_binary_size = sizeof(byte);			// type
			m_binary_size += sizeof(byte);		// flags
	    m_binary_size += sizeof( byte );	// width
	    m_binary_size += sizeof( byte );	// height
	    m_binary_size += sizeof( byte );	// baseline
			m_binary_size += sizeof( byte );  // horizontal character gap
	    m_binary_size += sizeof( byte );	// minascii
	    m_binary_size += sizeof( byte );	// maxascii
	    m_binary_size += sizeof( byte );  // defaultchar
  	  m_binary_size += sizeof( UInt16 );	// unicode char count

	    // go through all characters
	    for( i = 0; i < m_characters.Length; i++ )
	    {
		    if( m_characters[i].code <= 0xff )
		    {
			    m_binary_size += sizeof( UInt16 );	// table entry
		    }
		    else
		    {
			    m_binary_size += sizeof( UInt16 );	// character code
			    m_binary_size += sizeof( UInt16 );	// character address
		    }

		    m_binary_size += sizeof( byte );  						  // character width	
		    m_binary_size += m_characters[i].bitmap.Length;	// character data
      }
    }

		if (success && m_parent_class.VerboseMessages)
		{
			Console.WriteLine(resString.VerboseFont, in_filename, m_width, m_height, m_binary_size);
		}

		return success;
  }

  /// <summary>
  /// Reads next line from the text file and skips comment lines
  /// </summary>
  /// <param name="in_file"></param>
  /// <returns></returns>
  string ReadLineSkipComment( StreamReader in_file )
  {
	  string buffer;

	  do
	  {
		  buffer = in_file.ReadLine();
		  m_line_number++;

      if( buffer != null )
      {
		    buffer = buffer.Trim();

		    if( buffer.Length > 0 && buffer[0] == ';' )
			    buffer = "";
      }

	  }	while( buffer != null && buffer.Length == 0 );

	  return buffer;
  }

  /// <summary>
  /// Loads ASCII FNA font format file
  /// </summary>
  /// <param name="in_name"></param>
  /// <returns></returns>
  bool LoadFNA( string in_name )
  {
	  string[] attribute_name =
	  {
		  "name", "family", "isfixed", "width", "height", "minchar",
		  "maxchar", "baseline", "undwidth", "avgwidth", "minwidth",
		  "maxwidth", "defchar", "note", "horizontalchargap"
	  };

  	string[] family_name =
	  {
		  "roman", "swiss", "modern", "script", "decorative"
	  };

 	  bool success = true;
  	StreamReader file = null;
	  int attributes;
	  string buffer;
	  string[] command;
	  int number = 0;
	  int i,j;
	  int minchar = 0;
	  int maxchar = 0;
	  int default_char = -1;
	  int code;

  	// init
	  attributes = 0;
		m_type = fntFT_MONOCHROME;
		m_horizontal_char_gap = 0;

	  // open file
    try
    {
      file = File.OpenText(in_name);
      m_line_number = 0;
    }
    catch
    {
      success = false;
    }

	  if( success )
	  {
		  // read header
		  do
		  {
        buffer = ReadLineSkipComment(file);

			  // if not eof
			  if( buffer != null )
			  {
          if (Char.IsLetterOrDigit(buffer[0]))
          {
            // separate attribute name from attribute
            command = buffer.Split(new char[] { ' ' });

            // find attribute name
            i = Array.IndexOf(attribute_name, command[0]);

            if (i == -1)
              success = false;

            // check for duplicated attribute
            if (success && (attributes & (1 << i)) != 0)
              success = false;

            // convert parameter to number
            if (success && i > 1 && i < 13)
            {
              number = 0;

              success = int.TryParse(command[1], out number);

              // check for negative number
              if (success && number < 0)
                success = false;
            }

            // store attribute
            if (success)
            {
              switch (i)
              {
                // name
                case 0:
                  m_name = command[1];
                  break;

                // family
                case 1:
                  {
                    j = Array.IndexOf(family_name, command[1]);

                    switch (j)
                    {
                      // roman
                      case 0:
                        m_flags += FF_ROMAN;
                        break;

                      // swiss
                      case 1:
                        m_flags += FF_SWISS;
                        break;

                      // modern
                      case 2:
                        m_flags += FF_MODERN;
                        break;

                      // script
                      case 3:
                        m_flags += FF_MODERN;
                        break;

                      // decorative
                      case 4:
                        m_flags += FF_DECORATIVE;
                        break;

                      default:
                        m_flags += FF_DONTCARE;
                        break;
                    }
                  }
                  break;

                // isfixed
                case 2:
                  if (number != 0)
                    m_flags += FF_FIXED;
                  break;

                // width
                case 3:
                  m_width = number;
                  break;

                // height
                case 4:
                  m_height = number;
                  break;

                // minchar
                case 5:
                  minchar = number;
                  break;

                // maxchar
                case 6:
                  maxchar = number;
                  break;

                // baseline
                case 7:
                  m_baseline = number;
                  break;

                // avgwidth
                case 9:
                  attributes |= 1 << 3;
                  m_width = 0;
                  break;
                 
                // defchar
                case 12:
                  default_char = number;
                  break;

								// horizontalchargap
								case 14:
									m_horizontal_char_gap = (byte)number;
									break;
              }

              attributes |= 1 << i;
            }

            buffer = "";
          }
          else
            break;
			  }
		  }	while( success && !file.EndOfStream );

		  // check if all important attributes are loaded
		  if( success && (attributes & 0xFF) != 0xFF)
		  	success = false;

		  // check character count
		  if( success && (maxchar - minchar + 1) <= 0 )
			  success = false;

		  // allocate characters
		  if( success )
			  m_characters = new CharInfo[ maxchar - minchar + 1 ];
			  
			// set default character
			if( default_char == -1 )
        m_default_character = minchar;
      else
        m_default_character = default_char;
		
	  	// load character bitmaps
		  for( code = minchar; code <= maxchar && success; code++ )
		  {
        m_characters[code - minchar] = new CharInfo();

			  m_characters[code-minchar].code		= code;
			  m_characters[code-minchar].width	= -1;

  			for( i = 0; i < m_height && success; i++ )
	  		{
          if( buffer.Length == 0 )
            buffer = ReadLineSkipComment(file);

				  // check for eof
				  if( buffer == null )
					  success = false;

				  // store char with and allocate storage at the first line
				  if( m_characters[code-minchar].width == -1 )
				  {
					  // store with at the first line
					  if( ( m_flags	& FF_FIXED ) != 0 )
					  {
						  if( buffer.Length != m_width )
							  success = false;
					  }

					  m_characters[code-minchar].width = buffer.Length;

				  	// allocate storage
					  m_characters[code-minchar].bitmap = new byte[(( m_characters[code-minchar].width + 7 ) / 8) * m_height ];
            Array.Clear(m_characters[code - minchar].bitmap, 0, m_characters[code - minchar].bitmap.Length);
      		}

					// check length
					if( success && m_characters[code - minchar].width != buffer.Length )
						success = false;
      		
				  // store bits from this line
				  for( j = 0; j < m_characters[code-minchar].width && success; j++)
				  {
					  if( buffer[j] == '#')
						  m_characters[code-minchar].bitmap[i * (( m_characters[code-minchar].width + 7 ) / 8) + (j >> 3) ] |= (byte)(1 << (7 - (j & 7)));
					  else 
					  {
						  if( buffer[j] != '.')
							  success = false;
					  }
				  }

          buffer = "";
			  }
		  }

		  // close file
		  file.Close();
	  }

  	return success;
  }

  /// <summary>
  /// Load FNT file
  /// </summary>
  /// <param name="in_name"></param>
  /// <returns></returns>
  bool LoadFNT( string in_name )
  {
  	bool success = true;
  	FNTFileHeader header = new FNTFileHeader();
	  char ch;
	  int minascii, maxascii;
	  int code;
	  int bytes;
  	int i, y;
    UInt16 word_buffer;

		// init
		m_type = fntFT_MONOCHROME;
		m_horizontal_char_gap = 0;

  	// open file
    FileStream fs = null;

    try
    {
      fs = new FileStream(in_name, FileMode.Open, FileAccess.Read);
    }
    catch
    {
      success = false;
    }

  	if( success )
	  {
      try
      {
        header = ReadStruct<FNTFileHeader>(fs);
      }
      catch
      {
        success = false;
      }
    }

    // check header
		if( header.dfVersion != 0x0200 || header.dfType != 0 )
			success = false;

		// read font name
		fs.Seek( header.dfFace, SeekOrigin.Begin );
		m_name = "";
		do
		{
      ch = (char)fs.ReadByte();
			if( ch != '\0' )
				m_name += ch;

		}	while( ch != '\0' );

		// store flags
		switch( header.dfPitchAndFamily & fntFF_MASK )
		{
			case fntFF_ROMAN:
				m_flags = FF_ROMAN;
				break;

			case fntFF_SWISS:
				m_flags = FF_SWISS;
				break;

			case fntFF_MODERN:
				m_flags = FF_MODERN;
				break;

			case fntFF_SCRIPT:
				m_flags = FF_SCRIPT;
				break;

			case fntFF_DECORATIVE:
				m_flags = FF_DECORATIVE;
				break;

			default:
				m_flags = FF_DONTCARE;
				break;
		}

		// Italic
		if( header.dfItalic != 0 )
			m_flags |= FF_ITALIC;

		// Bold
		if( header.dfWeight > 400 )
			m_flags |= FF_BOLD;

		// fixed width
		if( header.dfPixWidth == 0 )
			m_flags |= FF_FIXED;

		// height
		m_height	= header.dfPixHeight;

		// width
		if( header.dfPixWidth == 0 )
			m_width		= header.dfAvgWidth;
		else
			m_width		= header.dfPixWidth;

		// baseline
		m_baseline	= header.dfAscent;

		// minascii
		minascii		= header.dfFirstChar;

		// maxascii
		maxascii		= header.dfLastChar;

	  // allocate characters
	  if( success )
		  m_characters = new CharInfo[ maxascii - minascii + 1 ];
 
		// load character bitmap
		for( code = minascii; code <= maxascii && success; code++ )
		{
			m_characters[code - minascii] = new CharInfo();

			// store code
			m_characters[code - minascii].code = code;

			// read width
      fs.Seek(Marshal.SizeOf(header) + (code - minascii) * sizeof(UInt16) * 2, SeekOrigin.Begin);

      word_buffer = (byte)fs.ReadByte();
      word_buffer += (ushort)(256 * fs.ReadByte());
 			m_characters[code - minascii].width = word_buffer;

			// address
      word_buffer = (byte)fs.ReadByte();
      word_buffer += (ushort)(256 * fs.ReadByte());
			fs.Seek( word_buffer, SeekOrigin.Begin );
			
			// allocate bitmap
			bytes = (m_characters[code - minascii].width + 7 ) / 8;

			m_characters[code - minascii].bitmap = new byte[ bytes * m_height ];

			// load bitmap
			for(i = 0; i < bytes; i++)
			{
				for(y = 0; y < m_height; y++)
				{
					m_characters[code - minascii].bitmap[bytes * y + i] = (byte)fs.ReadByte();
				}
			}
		}
 
		// close file
		fs.Close();

  	return success;
  }

  /// <summary>
  /// Returns file position priority index.
  /// </summary>
  /// <returns></returns>
  public override int GetFilePositionPriority()
  {
    return 30;
  }
    
  #endregion
}
