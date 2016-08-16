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
// Resource file object class factory
///////////////////////////////////////////////////////////////////////////////

public class drfClassFactory
{
  /// <summary>
  /// Creates factory classes for each possible resource chunk type
  /// </summary>
  /// <param name="in_resource_file"></param>
  public drfClassFactory(drfDominoResourceFile in_resource_file)
  {
    in_resource_file.AddFactoryClass(new drfJavaClass());
    in_resource_file.AddFactoryClass(new drfWave());
    in_resource_file.AddFactoryClass(new drfBitmap());
    in_resource_file.AddFactoryClass(new drfFont());
    in_resource_file.AddFactoryClass(new drfString());
    in_resource_file.AddFactoryClass(new drfBinary());
    in_resource_file.AddFactoryClass(new drfCDecl());
  }
}

