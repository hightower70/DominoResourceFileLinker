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
// Bitmap resource file object
///////////////////////////////////////////////////////////////////////////////
using System;
using System.IO;
using System.Runtime.InteropServices;

/// <summary>
/// Bitmap type resource chunk
/// </summary>
public class drfBitmap : drfFileResource
{
	#region · Constants ·
	public const UInt32 ClassId = 0x53504d42;
	public const int ALIGNMENT_SHIFT = 6;
	#endregion

	#region · Types ·

	const uint BI_RGB = 0;
  
  [StructLayout(LayoutKind.Sequential,Pack = 1)]
  public struct BITMAPFILEHEADER
  {
    public UInt16 bfType;
    public UInt32 bfSize;
    public UInt16 bfReserved1;
    public UInt16 bfReserved2;
    public UInt32 bfOffBits;
  }

  [StructLayout(LayoutKind.Sequential, Pack = 1)]
  public struct BITMAPINFOHEADER
  {
    public UInt32 biSize;
    public Int32 biWidth;
    public Int32 biHeight;
    public Int16 biPlanes;
    public Int16 biBitCount;
    public UInt32 biCompression;
    public UInt32 biSizeImage;
    public Int32 biXPelsPerMeter;
    public Int32 biYPelsPerMeter;
    public UInt32 biClrUsed;
    public UInt32 biClrImportant;
  }

  [StructLayout(LayoutKind.Sequential, Pack = 1)]
  public struct RGBQUAD
  {
    public byte rgbBlue;
    public byte rgbGreen;
    public byte rgbRed;
    public byte rgbReserved;
  }


	public enum ByteOrder
	{
		LowFirst,
		HighFirst
	};

	#endregion

	#region · Data Members ·
	int m_width;
  int m_height;
  int m_source_bits_per_pixel;
	int m_target_bits_per_pixel;
	int m_alignment_bytes;
	ByteOrder m_target_byte_order = ByteOrder.LowFirst;

  RGBQUAD[] m_palette;
  byte[] m_bitmap_buffer;
	#endregion

	#region · Constructor&Destructor ·
	/// <summary>
	/// Constructor
	/// </summary>
	public drfBitmap() : base(ClassId,"bitmap")
  {
    m_file_name = "";
    m_width = 0;
    m_height = 0;
    m_source_bits_per_pixel = 0;
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
		// update binary data
		int pos = 0;

    pos = m_binary_buffer.Modify((UInt16 )m_width, pos);
    pos = m_binary_buffer.Modify((UInt16)m_height, pos);
    pos = m_binary_buffer.Modify((byte)(m_target_bits_per_pixel | (m_alignment_bytes << ALIGNMENT_SHIFT)), pos);
		pos = m_binary_buffer.Modify(m_bitmap_buffer, pos + m_alignment_bytes);

    return 0;
  }

  /// <summary>
  /// Prepare binary data
  /// </summary>
  /// <param name="in_message"></param>
  /// <returns></returns>
  private int msgPrepareBinaryData(drfmsgPrepareBinaryData in_message)
  {
		int header_length;

    // prepare binary data
    m_binary_buffer = new clsBinaryBuffer();

		header_length = sizeof(UInt16) + // width
										sizeof(UInt16) + // height
										sizeof(byte); // bit depth


		// align pixel buffer to word of double word bondary
		m_alignment_bytes = CalculateAlignmentByteCount(in_message.FilePos + header_length);

		m_binary_buffer.Length = header_length + m_alignment_bytes + m_bitmap_buffer.Length; // data

		return 0;
  }

	/// <summary>
	/// Calculates the number of bytes needed before the binary pixel data in order to be on word (up to 16 bits per pixel) or
	/// double word (up to 32 bits per pixel) boundary.
	/// </summary>
	/// <param name="in_binary_data_pos">Position of the binary pixel data in the resource file</param>
	/// <returns></returns>
	private int CalculateAlignmentByteCount(int in_binary_data_pos)
	{
		// no alignment when one pixel is less than one byte
		if (m_target_bits_per_pixel <= 8)
			return 0;

		// align to word boundary when one pixel occupies two bytes
		if(m_target_bits_per_pixel <= 16)
		{
			// calculate and return alignment
			return (in_binary_data_pos & 0x01);
		}

		// align to double word boundary
		return (4 - (in_binary_data_pos & 0x03)) & 0x03;
	}

	/// <summary>
	/// Processes command line switches
	/// </summary>
	/// <param name="in_message"></param>
	/// <returns></returns>
	private int msgProcessCommandLine(drfmsgProcessCommandLine in_message)
  {
    if (in_message.Command == "bitmap")
    {
      if (!IsExists(in_message.Parameter))
      {
        // check identifier
        if (SetErrorIfResourceIdExists(in_message.Identifier))
        {
          // add bitmap file
          drfBitmap bitmap_file = new drfBitmap();
          m_parent_class.AddClass(bitmap_file);

          if (bitmap_file.Load(in_message.Parameter))
          {
						// set target bits in pixel to the source value
						bitmap_file.m_target_bits_per_pixel = bitmap_file.m_source_bits_per_pixel;
			
						// check for bitmap format
						if (in_message.Options != null && in_message.Options.Length == 1)
						{
							switch (in_message.Options[0].ToUpper())
							{
								case "RGB565":
									bitmap_file.m_target_bits_per_pixel = 16;
									break;

								case "RGB565REV":
									bitmap_file.m_target_bits_per_pixel = 16;
									bitmap_file.m_target_byte_order = ByteOrder.HighFirst;
									break;

								default:
									m_parent_class.ErrorMessage = string.Format(resString.ErrorInvalidOption, in_message.Options[0]);
									break;
							}
						}

						// color conversion
						bitmap_file.ColorConversion();

            // set resource information
            bitmap_file.m_file_name = in_message.Parameter;
            bitmap_file.m_resource_id = in_message.Identifier;

            // register bitmap class in the header
            drfmsgRegisterFileHeader register_message = new drfmsgRegisterFileHeader();

            register_message.Entry = bitmap_file;
            register_message.Id = ClassId;

            m_parent_class.BroadcastMessage(register_message);
          }
          else
            m_parent_class.ErrorMessage = string.Format(resString.ErrorCantOpenResourceFile, in_message.Parameter);
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
    Console.WriteLine(resString.UsageBitmap);

    return 0;
  }

	#endregion

	#region · Member functions ·


	/// <summary>
	/// Convers color depth from source depth to target depth
	/// </summary>
	private void ColorConversion()
	{
		// color conversion
		if (m_source_bits_per_pixel != m_target_bits_per_pixel)
		{
			int source_bits_in_line = m_source_bits_per_pixel * m_width;
			int target_bits_in_line = m_target_bits_per_pixel * m_width;
			int source_bytes_in_line = (source_bits_in_line + 7) / 8;
			int target_bytes_in_line = (target_bits_in_line + 7) / 8;
			int source_palette_index;
			byte[] target_bitmap_buffer;
			RGBQUAD color;
			int target_pixel_index;

			color.rgbBlue = 0;
			color.rgbGreen = 0;
			color.rgbRed = 0;

			// allocate pixel buffer (size rounded up to byte)
			target_bitmap_buffer = new byte[target_bytes_in_line * m_height];

			// copy rows
			for (int y = 0; y < m_height; y++)
			{
				// copy columns
				for (int x = 0; x < m_width; x++)
				{
					switch(m_source_bits_per_pixel)
					{
						case 8:
							source_palette_index = m_bitmap_buffer[source_bytes_in_line * y + x * m_source_bits_per_pixel / 8];
							break;

						default:
							source_palette_index = -1;
							break;
					}

					if (source_palette_index != -1)
					{
						color = m_palette[source_palette_index];
					}
					else
					{
						int address = source_bytes_in_line * y + x * m_source_bits_per_pixel / 8;

						color.rgbBlue = m_bitmap_buffer[address++];
						color.rgbGreen = m_bitmap_buffer[address++];
						color.rgbRed = m_bitmap_buffer[address];
					}

					target_pixel_index = target_bytes_in_line * y + x* m_target_bits_per_pixel / 8;
					switch (m_target_bits_per_pixel)
					{
						case 16:
							{
								UInt16 target_pixel_color = (UInt16)(((color.rgbRed & 0xf8) << 8) + ((color.rgbGreen & 0xfc) << 3) + ((color.rgbBlue & 0xf8) >> 3));

								switch (m_target_byte_order)
								{
									case ByteOrder.LowFirst:
										target_bitmap_buffer[target_pixel_index] = (byte)(target_pixel_color & 0xff);
										target_bitmap_buffer[target_pixel_index + 1] = (byte)((target_pixel_color >> 8) & 0xff);
										break;

									case ByteOrder.HighFirst:
										target_bitmap_buffer[target_pixel_index] = (byte)((target_pixel_color >> 8) & 0xff);
										target_bitmap_buffer[target_pixel_index + 1] = (byte)(target_pixel_color & 0xff);
										break;
								}
							}
							break;
					}
				}
			}

			// update bitmap buffer
			m_bitmap_buffer = target_bitmap_buffer;
		}
	}

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
  /// Load data from file
  /// </summary>
  /// <param name="in_name"></param>
  /// <returns></returns>
  private bool Load(string in_name)
  {
    byte[] line_buffer;
    int bits_in_line;
    int bytes_in_line;
    int pos;
    bool success = true;
    BITMAPFILEHEADER bitmap_file_header = new BITMAPFILEHEADER();
    BITMAPINFOHEADER bitmap_info_header = new BITMAPINFOHEADER();
    FileStream fs = null;

    try
    {
      fs = new FileStream(in_name, FileMode.Open, FileAccess.Read);
    }
    catch
    {
      success = false;
    }

    // load file header
    if (success)
    {
      try
      {
        bitmap_file_header = ReadStruct<BITMAPFILEHEADER>(fs);
      }
      catch
      {
        success = false;
      }
    }

    // check magic number
    if (bitmap_file_header.bfType != 'B' + 256 * 'M')
      success = false;

    // load info header
    if (success)
      bitmap_info_header = ReadStruct<BITMAPINFOHEADER>(fs);

    // check size
    if (bitmap_info_header.biSize != Marshal.SizeOf(bitmap_info_header))
      success = false;

    // only uncompressed bitmaps are supported
    if (bitmap_info_header.biCompression != BI_RGB)
      success = false;

    // store info
    m_width = bitmap_info_header.biWidth;

    if (bitmap_info_header.biHeight < 0)
      m_height = -bitmap_info_header.biHeight;
    else
      m_height = bitmap_info_header.biHeight;

    m_source_bits_per_pixel = bitmap_info_header.biBitCount;

    // load palete
    if (success)
    {
      m_palette  = new RGBQUAD[bitmap_info_header.biClrUsed];

      for (int i = 0; i < m_palette.Length; i++)
        m_palette[i] = ReadStruct<RGBQUAD>(fs);
    }

    // load pixel data
    if (success)
    {
      // number of bits in one line
      bits_in_line = bitmap_info_header.biBitCount * bitmap_info_header.biWidth;
      bytes_in_line = (bits_in_line + 7) / 8;

      // allocate pixel buffer (size rounded up to byte)
      m_bitmap_buffer = new byte[bytes_in_line * bitmap_info_header.biHeight];

      // allocate line buffer (size rounded up to 32 bits)
      line_buffer = new byte[((bits_in_line + 31) / 32 * 4)];

      // locate pixel data in the file
      fs.Seek(bitmap_file_header.bfOffBits, SeekOrigin.Begin);

      // copy lines
      for (int i = 0; i < m_height; i++)
      {
        // read line buffer
        fs.Read(line_buffer, 0, line_buffer.Length);

        // always store from top to bottom
        if (bitmap_info_header.biHeight < 0)
          pos = i * bytes_in_line;
        else
          pos = (m_height - 1 - i) * bytes_in_line;

        // copy line buffer into pixel buffer
				Array.Copy(line_buffer, 0, m_bitmap_buffer, pos, bytes_in_line);
      }
    }

    // close file
    if (fs != null)
      fs.Close();

    // return 
    if (success)
      m_file_name = in_name;

		if (success && m_parent_class.VerboseMessages)
		{
			Console.WriteLine(resString.VerboseBitmap, in_name, m_width, m_height, m_source_bits_per_pixel);
		}

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
