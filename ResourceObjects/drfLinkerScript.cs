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
// Linker script handler class
///////////////////////////////////////////////////////////////////////////////
using System;
using System.Windows.Forms;
using System.Xml;

public class drfLinkerScript : drfBaseClass
{
  #region Type declaration
  /// <summary>
  /// Java native method info class
  /// </summary>
  public class JavaNativeMethodInfo
  {
    public string Name;
    public int Index;
  }

  /// <summary>
  /// Java callback method info class
  /// </summary>
  public class JavaCallbackMethodInfo
  {
    public string Name;
    public int Index;
  }
  
  #endregion

  // member variables
  #region Member variables
  string m_output_file_name;
  string m_linker_script_file_name;
  XmlDocument m_xml_script;
  #endregion

  #region Constructor&Destructor
  /// <summary>
  /// Constructor
  /// </summary>
  public drfLinkerScript() : base(0,"linkerscript")
  {
    m_output_file_name = "";
    m_linker_script_file_name = "";
  }
  #endregion

  #region Properties
  /// <summary>
  /// Returns output file name
  /// </summary>
  public string OutputFileName
  {
    get
    {
      return m_output_file_name;
    }
  }

  /// <summary>
  /// Returns file format string
  /// </summary>
  /// <returns></returns>
  public string OutputFileFormat
  {
    get
    {
      return GetLinkerSettings("fileformat");
    }
  }

  #endregion

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
      case drfmsgMessageBase.drfmsgProcessCommandLine:
        return msgProcessCommandLine( (drfmsgProcessCommandLine)in_message);

      case drfmsgMessageBase.drfmsgDisplayHelpMessage:
        return msgDisplayHelpMessage( (drfmsgDisplayHelpMessage)in_message);
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
    Console.WriteLine(resString.UsageLinkerScript);

    return 0;
  }

  /// <summary>
  /// Process command line parameter
  /// </summary>
  /// <param name="in_message"></param>
  /// <returns></returns>
  private int msgProcessCommandLine(drfmsgProcessCommandLine in_message)
  {
    // check for linker script parameter
    if (in_message.Command == "linkerscript")
    {
      m_linker_script_file_name = in_message.Parameter;
      in_message.Used = true;
    }

    // check for output file name parameter
    if (in_message.Command == "output")
    {
      m_output_file_name = in_message.Parameter;
      in_message.Used = true;
    }

    // process verbose command line switch
    if (in_message.Command == "verbose")
    {
      m_parent_class.SetVerboseMessageFlag(true);
      in_message.Used = true;
    }

    // no more command line parameter
    if(in_message.Command == null )
    {
      // prepare linker script file name
      if (m_linker_script_file_name == "")
      {
        m_linker_script_file_name = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(Application.ExecutablePath), "lsdefault.xml");
      }

      // load linker script
      try
      {
        m_xml_script = new XmlDocument();
        m_xml_script.Load(m_linker_script_file_name);
      }
      catch
      {
        m_parent_class.ErrorMessage = resString.ErrorLinkerScriptLoadError;
      }

      // check linker script
      if (!m_parent_class.IsError())
      {
        if (m_xml_script.DocumentElement.Name != "linkerscript")
        {
          m_parent_class.ErrorMessage = resString.ErrorLinkerScriptLoadError;
        }
      }
    }

    return 0;
  }
  #endregion

  #region Linker script information access functions
  /// <summary>
  /// Gets linker settings
  /// </summary>
  /// <param name="in_key">In key to find</param>
  /// <returns>Key value</returns>
  public string GetLinkerSettings(string in_key)
  {
    XmlNode root = m_xml_script.DocumentElement;
    XmlNode node = root.SelectSingleNode("/linkerscript/linker/" + in_key);

    return node.InnerText;
  }

  /// <summary>
  /// Gets Java Native Method Information
  /// </summary>
  /// <param name="in_name"></param>
  /// <returns></returns>
  public JavaNativeMethodInfo GetJavaNativeMethodInfo(string in_name)
  {
    XmlNode root = m_xml_script.DocumentElement;
    XmlNode node = root.SelectSingleNode("java_native_methods/method[name='" + in_name + "']");

    // if there is no method info in the linker script file
    if (node == null)
      return null;

    // create return info
    JavaNativeMethodInfo retval = new JavaNativeMethodInfo();

    retval.Name = node.SelectSingleNode("name").InnerText;
   
    if( !int.TryParse(node.SelectSingleNode("index").InnerText, out retval.Index))
      return null;

    return retval;
  }

  /// <summary>
  /// Gets Java Native Method Information
  /// </summary>
  /// <param name="in_name"></param>
  /// <returns></returns>
  public JavaCallbackMethodInfo GetJavaCallbackMethodInfo(string in_name)
  {
    XmlNode root = m_xml_script.DocumentElement;
    XmlNode node = root.SelectSingleNode("java_callback_methods/method[name='" + in_name + "']");

    // if there is no method info in the linker script file
    if (node == null)
      return null;

    // create return info
    JavaCallbackMethodInfo retval = new JavaCallbackMethodInfo();

    retval.Name = node.SelectSingleNode("name").InnerText;

    if (!int.TryParse(node.SelectSingleNode("index").InnerText, out retval.Index))
      return null;

    return retval;
  }

  /// <summary>
  /// Gets highest callback method index
  /// </summary>
  /// <returns></returns>
  public int GetMaxCallbackMethodIndex()
  {
    XmlNode root = m_xml_script.DocumentElement;
    XmlNode method_node = root.SelectSingleNode("java_callback_methods");
    int max_index = 0;
    int index;

    foreach (XmlNode node in method_node.ChildNodes)
    {
      if (int.TryParse(node.SelectSingleNode("index").InnerText, out index))
      {
        if (index > max_index)
          max_index = index;
      }
    }

    return max_index;
  }
  #endregion

  /// <summary>
  /// Returns file position priority index.
  /// </summary>
  /// <returns></returns>
  public override int GetFilePositionPriority()
  {
    return 1000;
  }

}

