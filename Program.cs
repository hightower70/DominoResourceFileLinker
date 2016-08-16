using System;
using System.Collections.Generic;
using System.Text;

namespace OBCLinker
{
  class Program
  {
    // Main function
    static int Main(string[] args)
    {
      drfmsgMessageBase message;

      // display title
      Console.Write(resString.ProgramTitle);

      // create resource file class
      drfDominoResourceFile resource_file = new drfDominoResourceFile();

      // add factory classes
      new drfClassFactory(resource_file);

      // process command line
      sysCommandLine command_line = new sysCommandLine();
      if (!command_line.ProcessCommandLine(args))
      {
        resource_file.ErrorMessage = command_line.ErrorMessage;
      }

      // display help text
      if (!resource_file.IsError())
      {
        if (command_line.DisplayHelpIfRequested())
        {
          // broadcast help message message
          message = new drfmsgDisplayHelpMessage();
          resource_file.BroadcastFactoryMessage(message);

          Console.ReadKey();

          return 0;
        }
      }

      // process command line switches
      if (!resource_file.IsError())
      {
        sysCommandLine.CommandLineParameters[] parameters = command_line.Parameters;

        for (int i = 0; i < parameters.Length; i++)
        {
          message = new drfmsgProcessCommandLine(ref parameters[i]);
          resource_file.BroadcastFactoryMessage(message);
          parameters[i].Used = ((drfmsgProcessCommandLine)message).Used;
        }

        // check if all parameters has been used
        for (int i = 0; i < parameters.Length; i++)
        {
          if (!parameters[i].Used)
          {
            resource_file.ErrorMessage = string.Format(resString.ErrorUnusedParameter, parameters[i].Command, parameters[i].Parameter);
          }
        }
      }

      // send an empy message as closing the command line
      if (!resource_file.IsError())
      {
        sysCommandLine.CommandLineParameters closing_parameter = new sysCommandLine.CommandLineParameters();
        message = new drfmsgProcessCommandLine(ref closing_parameter);
        resource_file.BroadcastFactoryMessage(message);
      }

      // prepare binary data
      if (!resource_file.IsError())
      {
        message = new drfmsgPrepareBinaryData();
        resource_file.BroadcastMessage(message);
      }

      // link entries
      if (!resource_file.IsError())
      {
        message = new drfmsgLinkEntries();
        resource_file.BroadcastMessage(message);
      }

      // update binary data
      if (!resource_file.IsError())
      {
        message = new drfmsgUpdateBinaryData();
        resource_file.BroadcastMessage(message);
      }

      // update CRC
      if (!resource_file.IsError())
      {
        message = new drfmsgUpdateCRC();
        resource_file.BroadcastMessage( message);
      }

      // save resource file
      if (!resource_file.IsError())
        resource_file.Save();

      // Display error message
      if(resource_file.IsError())
      {
        Console.WriteLine(resString.ErrorPrefix + resource_file.ErrorMessage);
        Console.ReadKey();

        return 1;
      }

      // display resource file summary
      Console.WriteLine(string.Format(resString.StatusResourceFile,resource_file.LinkerScript.OutputFileName,resource_file.LinkerScript.OutputFileFormat,resource_file.GetBinarySize()));

      Console.ReadKey();

      return 0;
    }
  }
}
