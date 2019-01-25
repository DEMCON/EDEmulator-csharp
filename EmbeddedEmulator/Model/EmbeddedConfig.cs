﻿/*
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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EmbeddedEmulator.Model
{
    public class EmbeddedConfig
    {
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
    }
}
