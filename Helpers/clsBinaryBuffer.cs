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
// Binary data buffer class
///////////////////////////////////////////////////////////////////////////////
using System;

public class clsBinaryBuffer
{
	private byte[] m_buffer;

	public clsBinaryBuffer()
	{
		m_buffer = new byte[0];
	}

	public byte[] Buffer
	{
		get
		{
			return m_buffer;
		}
	}

	public int Length
	{
		set
		{
			Array.Resize(ref m_buffer, value);
		}

		get
		{
			return m_buffer.Length;
		}
	}

	/// <summary>
	/// Modifies byte of the buffer
	/// </summary>
	/// <param name="in_data"></param>
	/// <param name="in_pos"></param>
	public int Modify(byte in_data, int in_pos)
	{
		m_buffer[in_pos++] = in_data;

		return in_pos;
	}

	/// <summary>
	/// Adds UInt16 to the buffer
	/// </summary>
	/// <param name="in_data"></param>
	/// <param name="in_pos"></param>
	public int Modify(UInt16 in_data, int in_pos)
	{
		m_buffer[in_pos++] = (byte)(in_data % 256);
		m_buffer[in_pos++] = (byte)(in_data / 256);

		return in_pos;
	}

	/// <summary>
	/// Modifies UInt32 to the buffer
	/// </summary>
	/// <param name="in_data"></param>
	/// <param name="in_pos"></param>
	public int Modify(UInt32 in_data, int in_pos)
	{
		m_buffer[in_pos++] = (byte)(in_data);
		m_buffer[in_pos++] = (byte)(in_data >> 8);
		m_buffer[in_pos++] = (byte)(in_data >> 16);
		m_buffer[in_pos++] = (byte)(in_data >> 24);

		return in_pos;
	}

	/// <summary>
	/// Modifies Int32 to the buffer
	/// </summary>
	/// <param name="in_data"></param>
	/// <param name="in_pos"></param>
	public int Modify(Int32 in_data, int in_pos)
	{
		m_buffer[in_pos++] = (byte)(((UInt32)in_data));
		m_buffer[in_pos++] = (byte)(((UInt32)in_data) >> 8);
		m_buffer[in_pos++] = (byte)(((UInt32)in_data) >> 16);
		m_buffer[in_pos++] = (byte)(((UInt32)in_data) >> 24);

		return in_pos;
	}

	/// <summary>
	/// Adds Int32 to the end of the buffer
	/// </summary>
	/// <param name="in_data"></param>
	/// <param name="in_pos"></param>
	public int Add(Int32 in_data, int in_pos)
	{
		in_pos = Length;
		Length += sizeof(Int32);
		return Modify(in_data, in_pos);
	}

	/// <summary>
	/// Modifies buffer content using another clsByteBuffer content
	/// </summary>
	/// <param name="in_buffer"></param>
	/// <param name="in_pos"></param>
	public int Modify(clsBinaryBuffer in_buffer, int in_pos)
	{
		in_buffer.m_buffer.CopyTo(m_buffer, in_pos);

		return in_pos + in_buffer.Length;
	}

	/// <summary>
	/// Modifies buffer content using byte[] content
	/// </summary>
	/// <param name="in_buffer"></param>
	/// <param name="in_pos"></param>
	/// <returns></returns>
	public int Modify(byte[] in_buffer, int in_pos)
	{
		in_buffer.CopyTo(m_buffer, in_pos);

		return in_pos + in_buffer.Length;
	}

	/// <summary>
	/// Fill buffer with the specified data
	/// </summary>
	/// <param name="in_data"></param>
	public void Fill(byte in_data)
	{
		for (int i = 0; i < m_buffer.Length; i++)
			m_buffer[i] = in_data;
	}
}


