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
// String resources
///////////////////////////////////////////////////////////////////////////////

public class resString
{
  // program title
  public const string ProgramTitle = "*** Embedded Resource Linker v:1.1  Copyright by Laszlo Arvai 2005-2016 ***\n";

  // usage messages
  public const string Usage = "Usage: DominoRC <-switches> <switchfile>\n The 'switchfile' is a text file and one line of this file must contain one\n command line switch.\n Supported switches:\n -help or -? - Displays help message";
  public const string UsageLinkerScript = " -output:<filename.rom> : Sets output file name\n   Without this switch no output file is generated.\n -linkerscript:<linkerscript.xml> : Uses alternative linker script file.\n   If this switch is not specified the default script file 'lsdefault.xml'\n   in the current directory will be used. The file must be XML file.";
  public const string UsageJavaClass = " -class:<filename.class> : Adds a Java class to the resource file";
  public const string UsageWave = " -wave:<filename.wav> : Adds a Wave file to the resource file";
  public const string UsageBitmap = " -bitmap:<filename.bmp> : Adds a Bitmap file to the resource file";
  public const string UsageFont = " -font:<filename.fna> or <filename.fnt> : Adds a font (ASCII FNA or\n   binary FNT format) file to the resource file";
  public const string UsageCDecl = " -cdecl:<filename.h> : Creates C language style declarations.\n";
  public const string UsageString = " -string:\"string\" : Adds strings.\n";
  public const string UsageBinary = " -binary:<filename.bin> : Adds binary file to the resource file.\n"; 
  public const string StatusResourceFile = "Success. File name:{0}, type:{1}, size:{2} bytes";
  
  // error messages
  public const string ErrorPrefix = "ERROR: ";
  public const string ErrorUnusedParameter = "Unused or unknown parameter: -{0}:{1}";
  public const string ErrorCantOpenParameterFile = "Can't open parameter file. ({0})";
  public const string ErrorCantOpenResourceFile = "Can't open resource file. ({0})";
  public const string ErrorCantReadFile = "Can't read file. ({0})";
  public const string ErrorDuplicatedParameterFile = "Duplicated parameter file: ({0}).";
  public const string ErrorLinkerScriptLoadError = "Linker script load error";
  public const string ErrorJavaMethodNotFound = "Java method not found ({0}.{1})";
  public const string ErrorJavaNativeMethodNotFound = "Java native method not found ({0})";
  public const string ErrorFileFormatOrNameNotSpecified = "Resource file format or name not specified";
  public const string ErrorInvalidWaveFileParameter = "Invalid WAV file parameters (only 8kHz, 8bit, mono supported)";
  public const string ErrorResourceIdentifierExsist = "Resource identifier already exists. ({0})";
  public const string ErrorInvalidFNAFile = "FNA file ({0}) is invalid at line {1}.";
	public const string ErrorInvalidOption = "Invalid option ({0}).";

  // verbose messages
  public const string VerboseWave = "{0} - Wave file loaded ({1}Hz/{2}ch/{3}bit/{4}samples).";
	public const string VerboseBitmap = "{0} - Bitmap file loaded ({1}(w)x{2}(h), {3}bpp).";
	public const string VerboseBinary = "{0} - Binary file loaded ({1} bytes).";
	public const string VerboseFont = "{0} - Font file loaded ({1}(w)x{2}(h), {3} bytes).";
}