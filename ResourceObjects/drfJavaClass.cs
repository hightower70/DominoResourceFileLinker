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
// Java class handler
///////////////////////////////////////////////////////////////////////////////
using System;
using System.IO;

public class drfJavaClass : drfBaseClass
{
  #region Data types

  public const UInt32 ClassId = 0x534C434A;

  /// <summary>
  /// Access flags
  /// </summary>
  const int ACC_PUBLIC = 0x01;
  const int ACC_PRIVATE = 0x02;
  const int ACC_PROTECTED = 0x04;
  const int ACC_STATIC = 0x08;
  const int ACC_FINAL = 0x10;
  const int ACC_SYNCHRONIZED = 0x20;
  const int ACC_VOLATILE = 0x40;
  const int ACC_TRANSIENT = 0x80;
  const int ACC_NATIVE = 0x100;
  const int ACC_INTERFACE = 0x200;
  const int ACC_ABSTRACT = 0x400;

  /// <summary>
  /// Constant pool entry types
  /// </summary>
  enum ConstantPoolEntryType
  {
    CONSTANT_unknown = 0,
    CONSTANT_Utf8 = 1,
    CONSTANT_Integer = 3,
    CONSTANT_Float = 4,
    CONSTANT_Long = 5,
    CONSTANT_Double = 6,
    CONSTANT_String = 7,
    CONSTANT_Class = 8,
    CONSTANT_Fieldref = 9,
    CONSTANT_Methodref = 10,
    CONSTANT_InterfaceMethodref = 11,
    CONSTANT_NameAndType = 12
  };

  /// <summary>
  /// General storage for constant pool entry
  /// </summary>
  struct ConstantPoolEntry
  {
    public ConstantPoolEntryType Type;

    public string StringData;

    public UInt16 WordIndexL;
    public UInt16 WordIndexH;

    public Int32 IntData;
    public Int64 LongData;
    public float FloatData;
    public double DoubleData;

    public drfBaseClass MethodrefClass;
    public int MethodrefIndex;

    public UInt16 ConvertToBinary(clsBinaryBuffer in_buffer, ref int in_pos)
    {
      UInt16 retval;

      // some resource types has more bytes at the constants storage area
      switch (Type)
      {
        case ConstantPoolEntryType.CONSTANT_Utf8:
          break;

        case ConstantPoolEntryType.CONSTANT_Integer:
          retval = (UInt16)in_pos;
          in_pos = in_buffer.Add(IntData, in_pos);
          return retval;

        case ConstantPoolEntryType.CONSTANT_Float:
          break;

        case ConstantPoolEntryType.CONSTANT_Long:
          break;

        case ConstantPoolEntryType.CONSTANT_Double:
          break;

        case ConstantPoolEntryType.CONSTANT_String:
          break;

        case ConstantPoolEntryType.CONSTANT_Class:
          break;

        case ConstantPoolEntryType.CONSTANT_Fieldref:
          break;

        case ConstantPoolEntryType.CONSTANT_Methodref:
        {
          // check for java/lang/Object.<init>
          if (MethodrefClass == null && MethodrefIndex == 0)
            return 0;
          else
          {
            // other mehods
            if (MethodrefClass != null)
            {
              drfmsgGetJavaMethodChunkPos message = new drfmsgGetJavaMethodChunkPos();

              message.ClassEntry = (drfJavaClass)MethodrefClass;
              message.MethodIndex = MethodrefIndex;

              message.ClassEntry.ProcessMessage(message);

              return (UInt16)message.MethodPos;
            }
          }
        }
        break;

        case ConstantPoolEntryType.CONSTANT_InterfaceMethodref:
          break;

        case ConstantPoolEntryType.CONSTANT_NameAndType:
          break;

        default:
          break;
      }

      return 0xffff;
    }
  };

  /// <summary>
  /// Attributes table entry
  /// </summary>
  struct AttributesEntry
  {
    public UInt16 NameIndex;
    public byte[] Info;
  };

  /// <summary>
  /// Fields table entry
  /// </summary>
  struct FieldsTableEntry
  {
    public UInt16 AccessFlag;
    public UInt16 NameIndex;
    public UInt16 DescriptorIndex;
    public AttributesEntry[] Attributes;
  };

  /// <summary>
  /// Exception table entry
  /// </summary>
  struct ExceptionTableEntry
  {
    public UInt16 StartPC;
    public UInt16 EndPC;
    public UInt16 HandlerPC;
    public UInt16 CatchType;
  };

  /// <summary>
  /// Method table entry
  /// </summary>
  struct MethodTableEntry
  {
    // link info
    public bool Referenced;

    // header
    public UInt16 AccessFlag;
    public UInt16 NameIndex;
    public UInt16 DescriptorIndex;

    // code attribute
    public UInt16 CodeMaxStack;
    public UInt16 CodeMaxLocals;
    public byte[] CodeBytecode;
    public ExceptionTableEntry[] CodeExceptionTable;
    public AttributesEntry[] CodeAttributes;

    // other attributes
    public UInt16 AttributesCount;
    public AttributesEntry[] Attributes;

    // native method linkage
    public UInt32 NativeMethodIndex;
    public UInt32 StackRewind;

    public int GetBinarySize()
    {
      if ((AccessFlag & ACC_NATIVE) != 0)
      {
        // native methods
        return sizeof(UInt16) +					  // ClassAddress
                sizeof(UInt16) +					// AccessFlag
                sizeof(UInt16) +					// StackRewind
                sizeof(UInt16);					  // NativeMethodIndex
      }
      else
      {
        return sizeof(UInt16) +			  		// ClassAddress
                sizeof(UInt16) +					// AccessFlag
                sizeof(UInt16) +					// CodeMaxStack
                sizeof(UInt16) +					// CodeMaxLocal
                sizeof(UInt16) +          // BytecodeLength
                CodeBytecode.Length;  		// CodeBytecode
      }
    }
  };

  #endregion

  #region Data members

  // Binary file reader class (big endian)
  clsBinaryReader m_binary_reader;

  // Java data
  UInt32 m_magic;
  UInt16 m_minor_version;
  UInt16 m_major_version;
  ConstantPoolEntry[] m_constant_pool;
  UInt16 m_access_flags;
  UInt16 m_this_class;
  UInt16 m_super_class;
  UInt16[] m_interfaces;
  FieldsTableEntry[] m_fields;
  MethodTableEntry[] m_methods;
  AttributesEntry[] m_attributes;
  string m_class_path;
  drfJavaHeader m_java_header_class;
  int m_methods_pos;
  bool m_main_class;

  #endregion

  #region Constructor&Destructor
  /// <summary>
  /// Constructor
  /// </summary>
  public drfJavaClass() : base(ClassId,"java")
  {
    m_java_header_class = null;
    m_main_class = false;
  }

  #endregion

  public void SetMainClass()
  {
    m_main_class = true;
  }

  public void SetJavaHeaderClass(drfJavaHeader in_header_class)
  {
    m_java_header_class = in_header_class;
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
      case drfmsgMessageBase.drfmsgProcessCommandLine:
        return msgProcessCommandLine((drfmsgProcessCommandLine)in_message);

      case drfmsgMessageBase.drfmsgPrepareBinaryData:
        return msgPrepareBinaryData((drfmsgPrepareBinaryData)in_message);

      case drfmsgMessageBase.drfmsgUpdateBinaryData:
        return msgUpdateBinaryData((drfmsgUpdateBinaryData)in_message);

      case drfmsgMessageBase.drfmsgDisplayHelpMessage:
        return msgDisplayHelpMessage((drfmsgDisplayHelpMessage)in_message);

      case drfmsgMessageBase.drfmsgJavaFindMethod:
        return msgFindJavaMethod((drfmsgJavaFindMethod)in_message);

      case drfmsgMessageBase.drfmsgJavaFindClass:
        return msgJavaFindClass((drfmsgJavaFindClass)in_message);

      case drfmsgMessageBase.drfmsgLinkEntries:
        return msgLinkEntries((drfmsgLinkEntries)in_message);

      case drfmsgMessageBase.drfmsgGetJavaMethodChunkPos:
        return msgGetMethodChunkPos((drfmsgGetJavaMethodChunkPos)in_message);
    }

    return 0;
  }

  /// <summary>
  /// Link entries.
  /// </summary>
  /// <param name="in_message"></param>
  /// <returns></returns>
  private int msgLinkEntries(drfmsgLinkEntries in_message)
  {
    bool success = true;
    int i, j;

    // link native methods
    for (i = 0; i < m_methods.Length; i++)
    {
      // if method is native try to find it
      if ((m_methods[i].AccessFlag & ACC_NATIVE) != 0)
      {
        drfLinkerScript.JavaNativeMethodInfo method_info = m_parent_class.LinkerScript.GetJavaNativeMethodInfo(m_constant_pool[(int)m_methods[i].NameIndex].StringData);

        // if not found
        if (method_info == null)
        {
          m_parent_class.ErrorMessage = string.Format(resString.ErrorJavaNativeMethodNotFound, m_constant_pool[(int)m_methods[i].NameIndex].StringData);
          return 0;
        }

        // calculate stack rewind value
        int stack_rewind = 0;
        int stack_rewind_increment_sign = 0;
        string descriptor = m_constant_pool[(int)m_methods[i].DescriptorIndex].StringData;

        for (j = 0; j < descriptor.Length; j++)
        {
          switch (char.ToUpper(descriptor[j]))
          {
            // argumentum start
            case '(':
              stack_rewind_increment_sign = 1;
              break;

            // argumentum end (return value start)
            case ')':
              stack_rewind_increment_sign = -1;
              break;

            // byte
            case 'B':
              stack_rewind += stack_rewind_increment_sign;
              break;

            // character
            case 'C':
              stack_rewind += stack_rewind_increment_sign;
              break;

            // double
            case 'D':
              stack_rewind += stack_rewind_increment_sign * 2;
              break;

            // float
            case 'F':
              stack_rewind += stack_rewind_increment_sign;
              break;

            // integer
            case 'I':
              stack_rewind += stack_rewind_increment_sign;
              break;

            // long
            case 'J':
              stack_rewind += stack_rewind_increment_sign * 2;
              break;

            // class instance	
            case 'L':
              stack_rewind += stack_rewind_increment_sign;
              break;

            // short
            case 'S':
              stack_rewind += stack_rewind_increment_sign;
              break;

            // boolean
            case 'Z':
              stack_rewind += stack_rewind_increment_sign;
              break;

            // void
            case 'V':
              break;
          }
        }

        // cache method information
        m_methods[i].NativeMethodIndex = (UInt16)method_info.Index;
        m_methods[i].StackRewind = (UInt16)stack_rewind;
      }
      else
      {
        // check if callback method
        if (m_main_class)
        {
          string name = m_constant_pool[m_methods[i].NameIndex].StringData;
          drfLinkerScript.JavaCallbackMethodInfo method_info;

          method_info = m_parent_class.LinkerScript.GetJavaCallbackMethodInfo(name);

          // if it's callback method update position
          if (method_info != null)
          {
            drfmsgGetJavaMethodChunkPos message = new drfmsgGetJavaMethodChunkPos();

            message.ClassEntry = this;
            message.MethodIndex = method_info.Index;
            message.MethodPos = 0;

            msgGetMethodChunkPos(message);

            m_java_header_class.SetCallbackMethod(method_info.Index, message.MethodPos);
          }
        }
      }
    }

    // link methods
    for (i = 0; i < m_constant_pool.Length && success; i++)
    {
      if (m_constant_pool[i].Type == ConstantPoolEntryType.CONSTANT_Methodref)
      {
        drfmsgJavaFindMethod message = new drfmsgJavaFindMethod();

        message.ClassName = m_constant_pool[(int)m_constant_pool[(int)m_constant_pool[i].WordIndexL].WordIndexL].StringData;
        message.MethodName = m_constant_pool[(int)m_constant_pool[(int)m_constant_pool[i].WordIndexH].WordIndexL].StringData;

        // check java/lang/Object.<init> method
        if (message.ClassName == "java/lang/Object" && message.MethodName == "<init>")
        {
          message.ClassEntry = null;
          message.MethodIndex = 0;
        }
        else
        {
          // if the method belongs to this class
          if (message.ClassName == m_constant_pool[(int)m_constant_pool[m_this_class].WordIndexL].StringData)
          {
            this.ProcessMessage(message);

            // try super class
            if (message.ClassEntry == null || message.MethodIndex == -1)
            {
              // change to super class name
              message.ClassName = m_constant_pool[(int)m_constant_pool[(int)m_super_class].WordIndexL].StringData;

              // check if loaded
              drfmsgJavaFindClass find_class = new drfmsgJavaFindClass();

              find_class.ClassName = message.ClassName;

              m_parent_class.BroadcastMessage(find_class);

              // if not loaded -> load it
              if (find_class.ClassEntry == null)
              {
                drfJavaClass java_class = new drfJavaClass();

                if (java_class.Load(GenerateClassDefaultPath(find_class.ClassName)))
                {
                  // parse
                  if (java_class.Parse())
                  {
                    // set header class
                    java_class.SetJavaHeaderClass(m_java_header_class);

                    // add to parent list
                    m_parent_class.AddClass(java_class);

                    // send prepare binary data message
                    drfmsgPrepareBinaryData prepare_message = new drfmsgPrepareBinaryData();
                    java_class.ProcessMessage(prepare_message);

                    // send update file pos message
                    //drfmsgUpdateFilePos update_file_pos_message = new drfmsgUpdateFilePos();
                    //m_parent_class.BroadcastMessage(update_file_pos_message);

                    // send message
                    java_class.ProcessMessage(message);
                  }
                }
              }
              else
              {
                // send message to the already loaded super class
                find_class.ClassEntry.ProcessMessage(message);
              }
            }
          }
          else
          {
            m_parent_class.BroadcastMessage(message);

            // if method can't be found
            if (message.ClassEntry == null)
            {
              string class_to_load = "";

              // load class
              class_to_load = GenerateClassDefaultPath(message.ClassName);
              drfJavaClass java_class = new drfJavaClass();

              if (java_class.Load(class_to_load))
              {
                if (java_class.Parse())
                {
                  // set header class
                  java_class.SetJavaHeaderClass(m_java_header_class);

                  // add to the class list
                  m_parent_class.AddClass(java_class);

                  // send prepare binary data message
                  drfmsgPrepareBinaryData prepare_message = new drfmsgPrepareBinaryData();
                  java_class.ProcessMessage(prepare_message);

                  // send update file pos message
                  //drfmsgUpdateFilePos update_file_pos_message = new drfmsgUpdateFilePos();
                  //m_parent_class.BroadcastMessage(update_file_pos_message);

                  // send message
                  java_class.ProcessMessage(message);
                }
              }
            }
          }
        }

        // if not found -> error
        if (message.MethodIndex == -1)
        {
          m_parent_class.ErrorMessage = string.Format(resString.ErrorJavaMethodNotFound, message.ClassName, message.MethodName);
          return 0;
        }
        else
        {
          // store method info
          m_constant_pool[i].MethodrefClass = message.ClassEntry;
          m_constant_pool[i].MethodrefIndex = message.MethodIndex;
        }
      }
    }

    // register java class in the header
    drfmsgRegisterFileHeader register_message = new drfmsgRegisterFileHeader();

    register_message.Entry = this;
    register_message.Id = ClassId;

    m_parent_class.BroadcastMessage(register_message);

    return 0;
  }

  /// <summary>
  /// Prepare binary data
  /// </summary>
  /// <param name="in_message"></param>
  /// <returns></returns>
  private int msgPrepareBinaryData(drfmsgPrepareBinaryData in_message)
  {
    int size;
    int i;
    clsBinaryBuffer buffer = new clsBinaryBuffer();
    int pos;

    // calculate binary data size
    size = 0;

    size += sizeof(UInt16); // class length
    size += sizeof(UInt16); // constant pool table address
    size += sizeof(UInt16); // constant storage area address
    size += sizeof(UInt16); // method storage area address

    // constant pool table size
    size += sizeof(UInt16) * m_constant_pool.Length;

    // constant storage size
    pos = 0;
    for (i = 0; i < m_constant_pool.Length; i++)
      m_constant_pool[i].ConvertToBinary(buffer, ref pos);
    size += pos;

    // methods size
    m_methods_pos = size;
    for (i = 0; i < m_methods.Length; i++)
      size += m_methods[i].GetBinarySize();

    // prepare binary data buffer
    m_binary_buffer = new clsBinaryBuffer();

    m_binary_buffer.Length = size;

    return 0;
  }

  /// <summary>
  /// Updates binary data
  /// </summary>
  /// <param name="in_message"></param>
  /// <returns></returns>
  private int msgUpdateBinaryData(drfmsgUpdateBinaryData in_message)
  {
    clsBinaryBuffer buffer = new clsBinaryBuffer();
    int pos = 0;
    int i;
    int class_length_pos = 0;
    int constant_pool_table_address_pos = 0;
    int constant_storage_area_address_pos = 0;
    int method_storage_area_address_pos = 0;
    int constant_storage_area_address = 0;
    UInt16 length;

    // class length
    class_length_pos = pos;
    pos = m_binary_buffer.Modify((UInt16)0, pos);

    // constant pool table address
    constant_pool_table_address_pos = pos;
    pos = m_binary_buffer.Modify((UInt16)0, pos);

    // constant storage area address
    constant_storage_area_address_pos = pos;
    pos = m_binary_buffer.Modify((UInt16)0, pos);

    // method storage address
    method_storage_area_address_pos = pos;
    pos = m_binary_buffer.Modify((UInt16)0, pos);

    // store constant pool table and prepare constant storage area
    m_binary_buffer.Modify((UInt16)pos, constant_pool_table_address_pos);
    constant_storage_area_address = pos + m_constant_pool.Length * sizeof(UInt16);
    for (i = 0; i < m_constant_pool.Length; i++)
    {
      pos = m_binary_buffer.Modify(m_constant_pool[i].ConvertToBinary(buffer, ref constant_storage_area_address), pos);
    }

    // store constant storage area
    m_binary_buffer.Modify((UInt16)pos, constant_storage_area_address_pos);
    pos = m_binary_buffer.Modify(buffer, pos);

    // update methods address
    m_binary_buffer.Modify((UInt16)pos, method_storage_area_address_pos);

    // store methods
    for (i = 0; i < m_methods.Length; i++)
    {
      // class_address
      pos = m_binary_buffer.Modify((UInt16)(m_file_pos - m_java_header_class.GetFilePosition()), pos);

      // Acces flag
      pos = m_binary_buffer.Modify((UInt16)m_methods[i].AccessFlag, pos);

      if ((m_methods[i].AccessFlag & ACC_NATIVE) != 0)
      {
        // Stack rewind
        pos = m_binary_buffer.Modify((UInt16)(m_methods[i].StackRewind), pos);

        // Native method index
        pos = m_binary_buffer.Modify((UInt16)m_methods[i].NativeMethodIndex, pos);
      }
      else
      {
        // Maximum stack depth	
        pos = m_binary_buffer.Modify((UInt16)m_methods[i].CodeMaxStack, pos);

        // Maximum local variables
        pos = m_binary_buffer.Modify((UInt16)m_methods[i].CodeMaxLocals, pos);

        // Bytecode length
        pos = m_binary_buffer.Modify((UInt16)m_methods[i].CodeBytecode.Length, pos);

        // Bytecode
        pos = m_binary_buffer.Modify(m_methods[i].CodeBytecode, pos);
      }
    }

    // update class length
    length = (UInt16)pos;
    pos = 0;
    m_binary_buffer.Modify(length, pos);

    return 0;
  }

  /// <summary>
  /// Displays help message
  /// </summary>
  /// <param name="in_message"></param>
  /// <returns></returns>
  private int msgDisplayHelpMessage(drfmsgDisplayHelpMessage in_message)
  {
    Console.WriteLine(resString.UsageJavaClass);

    return 0;
  }

  /// <summary>
  /// Processes command line switches
  /// </summary>
  /// <param name="in_message"></param>
  /// <returns></returns>
  private int msgProcessCommandLine(drfmsgProcessCommandLine in_message)
  {
    if (in_message.Command == "class")
    {
      // add java class header
      drfJavaHeader java_header = new drfJavaHeader();
      m_parent_class.AddClass(java_header);

      // register java header class in the header
      drfmsgRegisterFileHeader register_message = new drfmsgRegisterFileHeader();

      register_message.Entry = java_header;
      register_message.Id = ClassId;

      m_parent_class.BroadcastMessage(register_message);

      // add java class
      drfJavaClass java_class = new drfJavaClass();

      java_class.SetMainClass();
      m_parent_class.AddClass(java_class);
      java_class.SetJavaHeaderClass(java_header);

      java_class.Load(in_message.Parameter);
      java_class.Parse();

      in_message.Used = true;
    }

    return 0;
  }

  /// <summary>
  /// Find Java Method Message
  /// </summary>
  /// <param name="in_message"></param>
  /// <returns></returns>
  public int msgFindJavaMethod(drfmsgJavaFindMethod in_message)
  {
    int i;

    // check if method of this class is searched
    if (in_message.ClassName == m_constant_pool[(int)m_constant_pool[(int)m_this_class].WordIndexL].StringData )
    {
      // find method
      i = 0;
      while (i < m_methods.Length)
      {
        if (in_message.MethodName == m_constant_pool[(int)m_methods[i].NameIndex].StringData )
          break;

        i++;
      }

      // if found
      if (i < m_methods.Length)
      {
        in_message.ClassEntry   = this;
        in_message.MethodIndex  = i;
      }
    }

    return 0;
  }

  /// <summary>
  /// Find Java class.
  /// </summary>
  /// <param name="in_message"></param>
  /// <returns></returns>
  public int msgJavaFindClass(drfmsgJavaFindClass in_message)
  {
    if (in_message.ClassName == m_constant_pool[(int)m_constant_pool[m_this_class].WordIndexL].StringData)
      in_message.ClassEntry = this;

    return 0;
  }

  /// <summary>
  /// Finds java method file pos relative to the first java class in the file
  /// </summary>
  /// <param name="in_method_index"></param>
  /// <returns></returns>
  int msgGetMethodChunkPos(drfmsgGetJavaMethodChunkPos in_message)
  {
    int i;
    int java_chunk_pos;
    int chunk_pos;

    // get first entry file pos
    java_chunk_pos = m_java_header_class.GetFilePosition();

    // calculate method file pos
    chunk_pos = m_file_pos - java_chunk_pos;
    chunk_pos += m_methods_pos;
    for (i = 0; i < in_message.MethodIndex; i++)
      chunk_pos += m_methods[i].GetBinarySize();

    in_message.MethodPos = chunk_pos;

    return 0;
  }

#endregion

  #region Other functions
  /// <summary>
  /// Returns file position priority index.
  /// </summary>
  /// <returns></returns>
  public override int GetFilePositionPriority()
  {
    return 11;
  }

  public bool Load(string in_name)
  {
    // load binary file
    m_binary_reader = new clsBinaryReader();

    m_binary_reader.Load(in_name);
    m_class_path = in_name;

    // check if it's really a class file
    UInt32 magic = 0;
    m_binary_reader.Position = 0;
    m_binary_reader.ReadData(ref magic);
    if (magic != 0xcafebabe)
    {
      return false;
    }

    return true;
  }


  /// <summary>
  /// Parse loaded binary class file
  /// </summary>
  /// <returns></returns>
  public bool Parse()
  {
    byte byte_buffer = 0;
    UInt16 word_buffer = 0;
    UInt32 dword_buffer = 0;
    int i;

    // load class header
    m_binary_reader.Position = 0;
    m_binary_reader.ReadData(ref m_magic);
    m_binary_reader.ReadData(ref m_minor_version);
    m_binary_reader.ReadData(ref m_major_version);

    // constant pool
    m_binary_reader.ReadData(ref word_buffer);
    m_constant_pool = new ConstantPoolEntry[word_buffer];

    for (i = 1; i < m_constant_pool.Length; i++)
    {
      m_binary_reader.ReadData(ref byte_buffer);
      m_constant_pool[i].Type = (ConstantPoolEntryType)byte_buffer;

      switch (m_constant_pool[i].Type)
      {
        case ConstantPoolEntryType.CONSTANT_Utf8:
          // init
          m_constant_pool[i].StringData = "";

          // convenient constants for checking and masking bits
          const byte HIGH_BIT = 0x80;
          const byte HIGH_2_BITS = 0xC0;
          //const byte HIGH_3_BITS = 0xE0;
          const byte LOW_4_BITS = 0x0F;
          const byte LOW_5_BITS = 0x1F;
          const byte LOW_6_BITS = 0x3F;

          // read length
          m_binary_reader.ReadData(ref word_buffer);

          UInt16 j = 0;
          while (j < word_buffer)
          {
            m_binary_reader.ReadData(ref byte_buffer);

            if ((byte_buffer & HIGH_BIT) == 0)
              m_constant_pool[i].StringData += (char)byte_buffer;
            else
            {
              UInt16 ch;

              if ((byte_buffer & HIGH_2_BITS) == HIGH_2_BITS)
              {
                ch = (UInt16)((byte_buffer & LOW_5_BITS) << 6);

                m_binary_reader.ReadData(ref byte_buffer);

                ch += (UInt16)(byte_buffer & LOW_6_BITS);

                j++;
              }
              else
              {
                ch = (UInt16)((byte_buffer & LOW_4_BITS) << 12);

                m_binary_reader.ReadData(ref byte_buffer);

                ch += (UInt16)((byte_buffer & LOW_6_BITS) << 6);

                m_binary_reader.ReadData(ref byte_buffer);

                ch += (UInt16)((byte_buffer & LOW_6_BITS));

                j += 2;
              }

              m_constant_pool[i].StringData += ch;
            }

            j++;
          }
          break;

        case ConstantPoolEntryType.CONSTANT_Integer:
          m_binary_reader.ReadData(ref m_constant_pool[i].IntData);
          break;

        case ConstantPoolEntryType.CONSTANT_Float:
          m_binary_reader.ReadData(ref m_constant_pool[i].FloatData);
          break;

        case ConstantPoolEntryType.CONSTANT_Long:
          m_binary_reader.ReadData(ref m_constant_pool[i].LongData);
          i++;
          m_constant_pool[i].Type = ConstantPoolEntryType.CONSTANT_unknown;
          break;

        case ConstantPoolEntryType.CONSTANT_Double:
          m_binary_reader.ReadData(ref m_constant_pool[i].DoubleData);
          i++;
          m_constant_pool[i].Type = ConstantPoolEntryType.CONSTANT_unknown;
          break;

        case ConstantPoolEntryType.CONSTANT_String:
          m_binary_reader.ReadData(ref m_constant_pool[i].WordIndexL);
          m_constant_pool[i].WordIndexH = 0;
          break;

        case ConstantPoolEntryType.CONSTANT_Class:
          m_binary_reader.ReadData(ref m_constant_pool[i].WordIndexL);
          m_constant_pool[i].WordIndexH = 0;
          break;

        case ConstantPoolEntryType.CONSTANT_Fieldref:
          m_binary_reader.ReadData(ref m_constant_pool[i].WordIndexL);
          m_binary_reader.ReadData(ref m_constant_pool[i].WordIndexH);
          break;

        case ConstantPoolEntryType.CONSTANT_Methodref:
          m_binary_reader.ReadData(ref m_constant_pool[i].WordIndexL);
          m_binary_reader.ReadData(ref m_constant_pool[i].WordIndexH);
          break;

        case ConstantPoolEntryType.CONSTANT_InterfaceMethodref:
          m_binary_reader.ReadData(ref m_constant_pool[i].WordIndexL);
          m_binary_reader.ReadData(ref m_constant_pool[i].WordIndexH);
          break;

        case ConstantPoolEntryType.CONSTANT_NameAndType:
          m_binary_reader.ReadData(ref m_constant_pool[i].WordIndexL);
          m_binary_reader.ReadData(ref m_constant_pool[i].WordIndexH);
          break;
      }
    }

    // access flag
    m_binary_reader.ReadData(ref m_access_flags);

    // this class
    m_binary_reader.ReadData(ref m_this_class);

    // super class
    m_binary_reader.ReadData(ref m_super_class);

    // load interfaces
    m_binary_reader.ReadData(ref word_buffer);

    m_interfaces = new UInt16[word_buffer];
    for (i = 0; i < m_interfaces.Length; i++)
    {
      m_binary_reader.ReadData(ref m_interfaces[i]);
    }

    // load fields
    m_binary_reader.ReadData(ref word_buffer);

    m_fields = new FieldsTableEntry[word_buffer];

    for (i = 0; i < m_fields.Length; i++)
    {
      // access flag
      m_binary_reader.ReadData(ref m_fields[i].AccessFlag);

      // name index
      m_binary_reader.ReadData(ref m_fields[i].NameIndex);

      // descriptor index
      m_binary_reader.ReadData(ref m_fields[i].DescriptorIndex);

      // attributes count
      m_binary_reader.ReadData(ref word_buffer);

      m_fields[i].Attributes = new AttributesEntry[word_buffer];

      for (int j = 0; j < m_fields[i].Attributes.Length; j++)
      {
        // name index
        m_binary_reader.ReadData(ref m_fields[i].Attributes[j].NameIndex);

        // info length
        m_binary_reader.ReadData(ref dword_buffer);
        m_fields[i].Attributes[j].Info = new byte[dword_buffer];

        // info
        m_binary_reader.ReadData(ref m_fields[i].Attributes[j].Info);
      }
    }

    // load methods
    m_binary_reader.ReadData(ref word_buffer);
    m_methods = new MethodTableEntry[word_buffer];

    for (i = 0; i < m_methods.Length; i++)
    {
      m_binary_reader.ReadData(ref m_methods[i].AccessFlag);
      m_binary_reader.ReadData(ref m_methods[i].NameIndex);
      m_binary_reader.ReadData(ref m_methods[i].DescriptorIndex);

      // init other members
      m_methods[i].CodeMaxStack = 0;
      m_methods[i].CodeMaxLocals = 0;
      m_methods[i].Referenced = false;

      // attributes count
      m_binary_reader.ReadData(ref m_methods[i].AttributesCount);

      // read attributes
      for (int j = 0; j < m_methods[i].AttributesCount; j++)
      {
        // name index
        m_binary_reader.ReadData(ref word_buffer);

        // check if code attribute
        if (m_constant_pool[word_buffer].StringData == "Code")
        {
          m_binary_reader.Position += 4; // skip length

          // read code attribute
          m_binary_reader.ReadData(ref m_methods[i].CodeMaxStack);
          m_binary_reader.ReadData(ref m_methods[i].CodeMaxLocals);

          // bytecode
          m_binary_reader.ReadData(ref dword_buffer);
          m_methods[i].CodeBytecode = new byte[dword_buffer];

          m_binary_reader.ReadData(ref m_methods[i].CodeBytecode);

          // exception table
          m_binary_reader.ReadData(ref word_buffer);
          m_methods[i].CodeExceptionTable = new ExceptionTableEntry[word_buffer];

          for (int k = 0; k < m_methods[i].CodeExceptionTable.Length; k++)
          {
            m_binary_reader.ReadData(ref m_methods[i].CodeExceptionTable[k].StartPC);
            m_binary_reader.ReadData(ref m_methods[i].CodeExceptionTable[k].EndPC);
            m_binary_reader.ReadData(ref m_methods[i].CodeExceptionTable[k].HandlerPC);
            m_binary_reader.ReadData(ref m_methods[i].CodeExceptionTable[k].CatchType);
          }

          // code attributes
          m_binary_reader.ReadData(ref word_buffer);
          m_methods[i].CodeAttributes = new AttributesEntry[word_buffer];

          for (int k = 0; k < m_methods[i].CodeAttributes.Length; k++)
          {
            m_binary_reader.ReadData(ref word_buffer);
            m_methods[i].CodeAttributes[k].NameIndex = word_buffer;

            m_binary_reader.ReadData(ref dword_buffer);
            m_methods[i].CodeAttributes[k].Info = new byte[dword_buffer];

            m_binary_reader.ReadData(ref m_methods[i].CodeAttributes[k].Info);
          }
        }
        else
        {
          // other attribute, simply load it
          Array.Resize(ref m_methods[i].Attributes, m_methods[i].Attributes.Length + 1);

          m_methods[i].Attributes[m_methods[i].Attributes.Length - 1].NameIndex = word_buffer;

          m_binary_reader.ReadData(ref dword_buffer);
          m_methods[i].Attributes[m_methods[i].Attributes.Length - 1].Info = new byte[dword_buffer];

          m_binary_reader.ReadData(ref m_methods[i].Attributes[m_methods[i].Attributes.Length - 1].Info);
        }
      }
    }

    // load attributes
    m_binary_reader.ReadData(ref word_buffer);
    m_attributes = new AttributesEntry[word_buffer];

    for (i = 0; i < m_attributes.Length; i++)
    {
      m_binary_reader.ReadData(ref word_buffer);
      m_attributes[i].NameIndex = word_buffer;

      m_binary_reader.ReadData(ref dword_buffer);
      m_attributes[i].Info = new byte[dword_buffer];

      m_binary_reader.ReadData(ref m_attributes[i].Info);
    }

    return true;
  }

  string GenerateClassDefaultPath(string in_class_name)
  {
    return Path.Combine(Path.GetDirectoryName(m_class_path), in_class_name) + ".class";
  }
  /*
  public byte[] GetCallbackMethodTable()
  {
    byte[] callback_method_table = new byte[0];
    int i, j;
    int pos = 0;

    for (i = 0; i < m_methods.Length; i++)
    {
      if (m_methods[i].CallbackMethodIndex != -1)
      {
        // resize table
        j = callback_method_table.Length;
        if (callback_method_table.Length <= m_methods[i].CallbackMethodIndex * sizeof(UInt16))
          Array.Resize(ref callback_method_table, (m_methods[i].CallbackMethodIndex+1) * sizeof(UInt16));

        // init table
        while (j < callback_method_table.Length)
        {
          callback_method_table[j] = 0xff;
          j++;
        }

        // copy index
        callback_method_table[m_methods[i].CallbackMethodIndex * sizeof(UInt16)] = (byte)(pos % 256);
        callback_method_table[m_methods[i].CallbackMethodIndex * sizeof(UInt16) + 1] = (byte)(pos / 256);
      }

      pos += m_methods[i].GetBinarySize();
    }

    return callback_method_table;
  }
  */
  #endregion
}
