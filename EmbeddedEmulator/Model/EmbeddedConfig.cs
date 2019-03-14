/*
Embedded Debugger PC Application which can be used to debug embedded systems at a high level.
Copyright (C) 2019 DEMCON advanced mechatronics B.V.

This program is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

This program is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with this program.  If not, see <http://www.gnu.org/licenses/>.
*/
using EmbeddedDebugger.DebugProtocol.Enums;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using System.Xml;

namespace EmbeddedEmulator.Model
{
    public class EmbeddedConfig
    {
        private static readonly Version currentVersion = new Version(0, 0, 0, 2);
        #region Properties
        private List<Register> readRegisters;
        public List<Register> ReadRegisters { get => readRegisters; set => readRegisters = value; }

        private List<Register> writeRegisters;
        public List<Register> WriteRegisters { get => writeRegisters; set => writeRegisters = value; }

        private string cpuName;
        public string CpuName { get => cpuName; set => cpuName = value; }

        private Version protocolVersion;
        public Version ProtocolVersion { get => protocolVersion; set => protocolVersion = value; }

        private Version applicationVersion;
        public Version ApplicationVersion { get => applicationVersion; set => applicationVersion = value; }

        private string serialNumber;
        public string SerialNumber { get => serialNumber; set => serialNumber = value; }

        public List<Register> Registers { get => new List<Register>().Concat(readRegisters).Concat(writeRegisters).OrderBy(x => x.ID).ToList(); }
        #endregion

        public EmbeddedConfig()
        {
            readRegisters = new List<Register>();
            writeRegisters = new List<Register>();
        }

        public static void PlaceConfigAsXML(EmbeddedConfig config)
        {
            XmlDocument doc = new XmlDocument();
            doc.AppendChild(doc.CreateXmlDeclaration("1.0", "UTF-8", null));

            // Add the root node for the XML document
            XmlElement root = doc.CreateElement("EmbeddedDebugger");
            root.SetAttribute("version", currentVersion.ToString());
            doc.AppendChild(root);

            XmlElement header = doc.CreateElement("Header");
            header.InnerText = "Embedded debugger, (C) DEMCON";
            root.AppendChild(header);

            // If there is information about the CPU present, add that
            XmlElement cpu = doc.CreateElement("CPU");
            root.AppendChild(cpu);

            XmlElement cpuName = doc.CreateElement("Name");
            cpuName.InnerText = "Embedded Debugger";
            cpu.AppendChild(cpuName);

            XmlElement version = doc.CreateElement("Version");
            cpu.AppendChild(version);

            XmlElement protVersion = doc.CreateElement("ProtocolVersion");
            protVersion.InnerText = "0.0.7";
            version.AppendChild(protVersion);

            XmlElement applicationVersion = doc.CreateElement("ApplicationVersion");
            applicationVersion.InnerText = "0.0.1";
            version.AppendChild(applicationVersion);


            // Start with the registers
            XmlElement registersAvailable = doc.CreateElement("RegistersAvailable");
            root.AppendChild(registersAvailable);

            foreach (Register reg in config.Registers)
            {
                registersAvailable.AppendChild(GetXMLFromRegister(reg, doc, (uint)config.Registers.IndexOf(reg)));
            }
            int id = 0;
            string seriallocation = @"C:\Configurations\" + @"Serial\" + $"{config.CpuName}\\" +
                $"/cpu{id.ToString("D2")}" +
                $"-V{config.ApplicationVersion.Major.ToString("D2")}" +
                $"_{config.ApplicationVersion.Minor.ToString("D2")}" +
                $"_{config.ApplicationVersion.Build.ToString("D4")}.xml";
            string tcplocation = @"C:\Configurations\" + @"TCP\" + $"{config.CpuName}\\" +
                $"/cpu{id.ToString("D2")}" +
                $"-V{config.ApplicationVersion.Major.ToString("D2")}" +
                $"_{config.ApplicationVersion.Minor.ToString("D2")}" +
                $"_{config.ApplicationVersion.Build.ToString("D4")}.xml";
            Directory.CreateDirectory(Path.GetDirectoryName(seriallocation));
            doc.Save(seriallocation);
            Directory.CreateDirectory(Path.GetDirectoryName(tcplocation));
            doc.Save(tcplocation);
        }

        /// <summary>
        /// Retrieve XML for a Register
        /// </summary>
        /// <param name="reg">The register</param>
        /// <param name="doc">The document</param>
        /// <param name="id">If no ID is present, this can be used</param>
        /// <returns>The generated XML element</returns>
        private static XmlElement GetXMLFromRegister(Register reg, XmlDocument doc, uint id)
        {

            // Create the XML node and add all information
            XmlElement register = doc.CreateElement("Register");
            register.SetAttribute("id", (reg.ID == 0 ? id : reg.ID).ToString());
            register.SetAttribute("name", reg.Name);
            register.SetAttribute("fullName", reg.FullName);
            register.SetAttribute("type", reg.VariableType.ToString().ToLower());
            if (reg.VariableType == VariableType.Unknown)
            {
                register.SetAttribute("unknownType", reg.VariableTypeName);
            }
            register.SetAttribute("size", reg.Size.ToString());
            register.SetAttribute("offset", reg.Offset.ToString());
            register.SetAttribute("source", reg.Source.ToString());
            register.SetAttribute("derefDepth", reg.DerefDepth.ToString());
            register.SetAttribute("show", Convert.ToInt32(true).ToString());
            register.SetAttribute("readWrite", reg.ReadWrite.ToString());

            return register;
        }
        public string GetCPPVariableTypes(VariableType varType)
        {
            switch (varType)
            {
                case VariableType.Bool:
                    return "boolean";
                case VariableType.SChar:
                    return "int8_t";
                case VariableType.Short:
                    return "int16_t";
                case VariableType.Int:
                    return "int32_t";
                case VariableType.Long:
                    return "int64_t";
                case VariableType.UChar:
                    return "uint8_t";
                case VariableType.UShort:
                    return "uint16_t";
                case VariableType.UInt:
                    return "uint32_t";
                case VariableType.ULong:
                    return "uint64_t";
            }
            return varType.ToString().ToLower();
        }
    }
}
