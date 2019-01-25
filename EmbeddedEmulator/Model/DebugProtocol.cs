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
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EmbeddedDebugger.Connectors.CustomEventArgs;
using EmbeddedDebugger.Connectors.Interfaces;
using EmbeddedDebugger.DebugProtocol;
using EmbeddedDebugger.DebugProtocol.Enums;
using EmbeddedDebugger.DebugProtocol.Messages;
using EmbeddedDebugger.Model.Messages;

namespace EmbeddedEmulator.Model
{
    public class DebugProtocol
    {
        private EmbeddedConfig embeddedConfig;
        private byte[] remainder;
        private bool autoRespond;
        private IConnector connector;
        private Stopwatch stopwatch;
        private List<IConnector> connectors;
        private System.Timers.Timer timer;
        private int timerCounter;
        private bool multipleNodes;
        private int numberOfNodesToSimulate;

        public bool AutoRespond { get => autoRespond; set => autoRespond = value; }
        public IConnector Connector
        {
            get => connector;
            set
            {
                if (connector != null)
                {
                    connector.MessageReceived -= MessageReceived;
                }
                connector = value;
                if (connector != null)
                {
                    connector.MessageReceived += MessageReceived;
                }
            }
        }
        public List<IConnector> Connectors { get => connectors; }
        public bool SimulateMultipleNodes { get => multipleNodes; set => multipleNodes = value; }
        public int NumberOfNodesToSimulate { get => numberOfNodesToSimulate; set => numberOfNodesToSimulate = value; }

        public event EventHandler<ProtocolMessage> NewWriteMessage = delegate { };
        public event EventHandler<string> NewDebugString = delegate { };



        public DebugProtocol(EmbeddedConfig embeddedConfig)
        {
            connectors = GetConnectorTypes().ToList();
            foreach(IConnector ic in connectors)
            {
                ic.AsServer = true;
            }
            this.embeddedConfig = embeddedConfig;
            remainder = new byte[0];
            autoRespond = true;
            stopwatch = new Stopwatch();
            stopwatch.Start();
            timer = new System.Timers.Timer(1)
            {
                AutoReset = true
            };
            timer.Elapsed += Timer_Elapsed;
        }

        ~DebugProtocol()
        {
            timer.Stop();
            timer.Dispose();
        }

        private void Timer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            timerCounter++;
            double value = (((double)stopwatch.ElapsedMilliseconds / 1000) % 6.28 - 3.14);
            byte sinByteValue = (byte)(100 * Math.Sin(value) + 128);
            double cosByteValue = 100 * Math.Cos(value) + 128;
            byte tanByteValue = (byte)(Math.Tan(value) + 128);
            byte random = (byte)new Random(Guid.NewGuid().GetHashCode()).Next(0, 255);
            //int counter = (int)embeddedConfig.ReadRegisters.First(x => x.FullName == "Counter").Value.Value;
            //embeddedConfig.ReadRegisters.First(x => x.FullName == "Counter").Value.ValueByteArray = BitConverter.GetBytes(counter + 1);
            embeddedConfig.ReadRegisters.First(x => x.FullName == "Sine").Value.ValueByteArray = new byte[] { sinByteValue };
            //embeddedConfig.ReadRegisters.First(x => x.FullName == "Cosine").Value.ValueByteArray = BitConverter.GetBytes(cosByteValue);
            //embeddedConfig.ReadRegisters.First(x => x.FullName == "Tangent").Value.ValueByteArray = new byte[] { tanByteValue };
            //embeddedConfig.ReadRegisters.First(x => x.FullName == "Random").Value.ValueByteArray = new byte[] { random };

            List<byte> data = new List<byte>
            {
                (byte)(stopwatch.ElapsedMilliseconds),
                (byte)(stopwatch.ElapsedMilliseconds >> 8),
                (byte)(stopwatch.ElapsedMilliseconds >> 16)
            };
            ushort mask = 0b0000_0000_0000_0000;
            if (!embeddedConfig.Registers.Any(x => x.IsDebugChannel && (x.ChannelMode == ChannelMode.LowSpeed || x.ChannelMode == ChannelMode.OnChange))) return;
            if (timerCounter % 5 == 0)
            {
                embeddedConfig.ReadRegisters.First(x => x.ID == 7).Value.ValueByteArray = BitConverter.GetBytes((char)embeddedConfig.ReadRegisters.First(x => x.ID == 7).Value.Value + 1);
                embeddedConfig.ReadRegisters.First(x => x.ID == 20).Value.ValueByteArray = BitConverter.GetBytes((sbyte)embeddedConfig.ReadRegisters.First(x => x.ID == 20).Value.Value + 1);
                embeddedConfig.ReadRegisters.First(x => x.ID == 9).Value.ValueByteArray = BitConverter.GetBytes((byte)embeddedConfig.ReadRegisters.First(x => x.ID == 9).Value.Value + 1);
                embeddedConfig.ReadRegisters.First(x => x.ID == 10).Value.ValueByteArray = BitConverter.GetBytes((short)embeddedConfig.ReadRegisters.First(x => x.ID == 10).Value.Value + 1);
                embeddedConfig.ReadRegisters.First(x => x.ID == 11).Value.ValueByteArray = BitConverter.GetBytes((ushort)embeddedConfig.ReadRegisters.First(x => x.ID == 11).Value.Value + 1);
                embeddedConfig.ReadRegisters.First(x => x.ID == 12).Value.ValueByteArray = BitConverter.GetBytes((int)embeddedConfig.ReadRegisters.First(x => x.ID == 12).Value.Value + 1);
                embeddedConfig.ReadRegisters.First(x => x.ID == 13).Value.ValueByteArray = BitConverter.GetBytes((uint)embeddedConfig.ReadRegisters.First(x => x.ID ==13).Value.Value + 1);
                embeddedConfig.ReadRegisters.First(x => x.ID == 14).Value.ValueByteArray = BitConverter.GetBytes((long)embeddedConfig.ReadRegisters.First(x => x.ID == 14).Value.Value + 1);
                embeddedConfig.ReadRegisters.First(x => x.ID == 15).Value.ValueByteArray = BitConverter.GetBytes((ulong)embeddedConfig.ReadRegisters.First(x => x.ID == 15).Value.Value + 1);
                //int counter = (int)embeddedConfig.ReadRegisters.First(x => x.FullName == "Counter").Value.Value;
                //embeddedConfig.ReadRegisters.First(x => x.FullName == "Counter").Value.ValueByteArray = BitConverter.GetBytes(counter+1);
            }
            if (timerCounter >= 20)
            {
                //byte counter = (byte)((byte)embeddedConfig.ReadRegisters.First(x => x.FullName == "Counter").Value.Value + 1);
                //embeddedConfig.ReadRegisters.First(x => x.FullName == "Counter").Value.ValueByteArray = new byte[] { counter };
                foreach (Register r in embeddedConfig.Registers.Where(x => x.IsDebugChannel &&
                    (x.ChannelMode == ChannelMode.LowSpeed || x.ChannelMode == ChannelMode.OnChange)).OrderBy(x => x.DebugChannel))
                {
                    if (!r.DebugChannel.HasValue) continue;
                    mask = (ushort)(mask | 1 << r.DebugChannel);
                    data.AddRange(r.Value.ValueByteArray);
                }
                //Console.WriteLine($"Sometimes: {embeddedConfig.ReadRegisters.First(x => x.ID == 20).Value.Value} Size: {embeddedConfig.ReadRegisters.First(x => x.ID == 20).Value.ValueByteArray.Length} ");
                timerCounter = 0;
            }
            else
            {
                if (!embeddedConfig.Registers.Any(x => x.IsDebugChannel && (x.ChannelMode == ChannelMode.OnChange))) return;
                foreach (Register r in embeddedConfig.Registers.Where(x => x.IsDebugChannel &&
                x.ChannelMode == ChannelMode.OnChange).OrderBy(x => x.DebugChannel))
                {
                    mask = (ushort)(mask | 1 << r.DebugChannel);
                    data.AddRange(r.Value.ValueByteArray);
                }
                //Console.WriteLine($"Often: {embeddedConfig.ReadRegisters.First(x => x.ID == 20).Value.Value} Size: {embeddedConfig.ReadRegisters.First(x => x.ID == 20).Value.ValueByteArray.Length} ");
            }
            data.Insert(3, (byte)mask);
            data.Insert(4, (byte)(mask >> 8));
            if (multipleNodes)
            {
                for (byte i = 0; i < numberOfNodesToSimulate; i++)
                {
                    SendTrace(EmbeddedDebugger.DebugProtocol.Enums.TraceLevel.Trace, "SendingChannelData has been called", i);
                    connector.SendMessage(MessageCodec.EncodeMessage(TemplateProvider.GetReadChannelDataMessage(data.ToArray(), i)));
                }
            }
            else
            {
                connector.SendMessage(MessageCodec.EncodeMessage(TemplateProvider.GetReadChannelDataMessage(data.ToArray(), 0x01)));
            }

        }

        /// <summary>
        /// This method gathers all classes extending either the IConnector or the IProjectConnector.
        /// </summary>
        /// <returns>The list of all connectors</returns>
        private IEnumerable<IConnector> GetConnectorTypes()
        {
            foreach (Type typeString in AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(s => s.GetTypes())
                .Where(p => typeof(IConnector).IsAssignableFrom(p) && !p.IsInterface).ToList())
            {
                yield return (IConnector)Activator.CreateInstance(typeString);
            }
        }

        public void MessageReceived(object sender, BytesReceivedEventArgs e)
        {
            MessageCodec.DecodeMessages(e.Message, out List<ProtocolMessage> msgs, out remainder, remainder);
            foreach (ProtocolMessage msg in msgs)
            {
                //Console.WriteLine(msg);
                if (!autoRespond) { connector.SendMessage(MessageCodec.EncodeMessage(TemplateProvider.GetAccMsg(msg))); }
                else if (msg.Valid)
                {
                    //Console.WriteLine(msg.Command.ToString());
                    switch (msg.Command)
                    {
                        case Command.GetVersion:
                            if (msg.ControllerID == 0xFF)
                            {
                                for (byte i = 0; i < numberOfNodesToSimulate; i++)
                                {
                                    SendTrace(EmbeddedDebugger.DebugProtocol.Enums.TraceLevel.Fatal, "Version has been called", i);
                                    connector.SendMessage(MessageCodec.EncodeMessage(TemplateProvider.GetVersionMessage(msg.MsgID, embeddedConfig, i)));
                                }
                            }
                            connector.SendMessage(MessageCodec.EncodeMessage(TemplateProvider.GetVersionMessage(msg.MsgID, embeddedConfig, 0x01)));
                            break;
                        case Command.GetInfo:
                            SendTrace(EmbeddedDebugger.DebugProtocol.Enums.TraceLevel.Error, "Info has been called");
                            connector.SendMessage(MessageCodec.EncodeMessage(TemplateProvider.GetInfoMessage(msg.MsgID, msg.ControllerID)));
                            break;
                        case Command.EmbeddedConfiguration:
                            SendTrace(EmbeddedDebugger.DebugProtocol.Enums.TraceLevel.Warning, "EmbeddedConfig has been called");
                            DispatchEmbeddedConfigurationMessage(msg);
                            break;
                        case Command.ConfigChannel:
                            SendTrace(EmbeddedDebugger.DebugProtocol.Enums.TraceLevel.Info, "ConfigChannel has been called");
                            DispatchConfigChannelMessage(msg);
                            break;
                        case Command.DebugString:
                            connector.SendMessage(MessageCodec.EncodeMessage(TemplateProvider.GetAccMsg(msg)));
                            DispatchDebugStringMessage(msg);
                            break;
                        case Command.WriteRegister:
                            DispatchWriteRegisterMessage(msg);
                            connector.SendMessage(MessageCodec.EncodeMessage(new ProtocolMessage(msg.ControllerID, msg.MsgID, msg.Command, new byte[] { 0x00 })));
                            break;
                        case Command.QueryRegister:
                            DispatchQueryRegisterMessage(msg);
                            break;
                        case Command.Decimation:
                        case Command.ResetTime:
                            DispatchResetTimeMessage(msg);
                            break;
                        case Command.ReadChannelData:
                            DispatchReadChannelDataMessage(msg);
                            break;
                    }
                }
            }
        }

        private void DispatchWriteRegisterMessage(ProtocolMessage msg)
        {
            uint offset = BitConverter.ToUInt32(msg.CommandData, 0);
            byte ctrl = msg.CommandData[4];
            MessageCodec.GetControlByteValues(embeddedConfig.ProtocolVersion, ctrl, out ReadWrite readWrite, out Source source, out int derefDepth);
            int size = msg.CommandData[5];
            byte[] value = msg.CommandData.Skip(6).ToArray();
            if (embeddedConfig.WriteRegisters.Any(x =>
                 x.Offset == offset &&
                 x.DerefDepth == derefDepth &&
                 x.ReadWrite == readWrite &&
                 x.Source == source &&
                 x.Size == size
            ))
            {
                embeddedConfig.WriteRegisters.First(x =>
                    x.Offset == offset &&
                    x.DerefDepth == derefDepth &&
                    x.ReadWrite == readWrite &&
                    x.Source == source &&
                    x.Size == size
                ).Value.ValueByteArray = value;
                NewWriteMessage(this, msg);
            }
        }

        private void DispatchConfigChannelMessage(ProtocolMessage msg)
        {
            ProtocolMessage returnMsg = new ProtocolMessage(msg.ControllerID, msg.MsgID, Command.ConfigChannel, new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 });
            if (msg.ControllerID != 0x01) { connector.SendMessage(MessageCodec.EncodeMessage(returnMsg)); return; }
            if (msg.CommandData.Length < 1)
            {
                returnMsg = new ProtocolMessage(msg.ControllerID, msg.MsgID, Command.ConfigChannel);
            }
            else
            {
                byte channelNumber = msg.CommandData[0];
                Register r = null;
                if (msg.CommandData.Length == 1 && embeddedConfig.Registers.Any(x => x.DebugChannel == channelNumber))
                {
                    r = embeddedConfig.Registers.First(x => x.DebugChannel == channelNumber);
                    List<byte> data = new List<byte>
                        {
                            (byte)r.ChannelMode
                        };
                    data.AddRange(BitConverter.GetBytes(r.Offset));
                    data.Add(MessageCodec.GetControlByte(embeddedConfig.ProtocolVersion, r.ReadWrite, r.Source, r.DerefDepth));
                    data.Add((byte)r.Size);
                    returnMsg = new ProtocolMessage(msg.ControllerID, msg.MsgID, Command.ConfigChannel, data.ToArray());

                }
                else if (msg.CommandData.Length == 2 && embeddedConfig.Registers.Any(x => x.DebugChannel == channelNumber))
                {
                    r = embeddedConfig.Registers.First(x => x.DebugChannel == channelNumber);
                    r.ChannelMode = (ChannelMode)msg.CommandData[1];
                    List<byte> data = new List<byte>
                    {
                        (byte)r.ChannelMode
                    };
                    returnMsg = new ProtocolMessage(msg.ControllerID, msg.MsgID, Command.ConfigChannel, data.ToArray());
                }
                else if (msg.CommandData.Length == 8)
                {
                    if (embeddedConfig.Registers.Any(x => x.DebugChannel == channelNumber))
                    {
                        embeddedConfig.Registers.First(x => x.DebugChannel == channelNumber).DebugChannel = null;
                    }
                    if (embeddedConfig.Registers.Any(
                        x => x.Offset == BitConverter.ToUInt32(msg.CommandData, 2) &&
                        MessageCodec.GetControlByte(embeddedConfig.ProtocolVersion, x.ReadWrite, x.Source, x.DerefDepth) == msg.CommandData[6] &&
                        x.Size == msg.CommandData[7]))
                    {
                        r = embeddedConfig.Registers.First(
                            x => x.Offset == BitConverter.ToUInt32(msg.CommandData, 2) &&
                            MessageCodec.GetControlByte(embeddedConfig.ProtocolVersion, x.ReadWrite, x.Source, x.DerefDepth) == msg.CommandData[6] &&
                            x.Size == msg.CommandData[7]);
                        r.DebugChannel = msg.CommandData[0];
                        r.ChannelMode = (ChannelMode)msg.CommandData[1];
                        returnMsg = msg;
                    }
                }
            }
            connector.SendMessage(MessageCodec.EncodeMessage(returnMsg));
        }

        private void DispatchQueryRegisterMessage(ProtocolMessage msg)
        {
            uint offset = BitConverter.ToUInt32(msg.CommandData, 0);
            byte ctrl = msg.CommandData[4];
            MessageCodec.GetControlByteValues(embeddedConfig.ProtocolVersion, ctrl, out ReadWrite readWrite, out Source source, out int derefDepth);
            int size = msg.CommandData[5];
            if (embeddedConfig.Registers.Any(x =>
                 x.Offset == offset &&
                 x.DerefDepth == derefDepth &&
                 x.ReadWrite == readWrite &&
                 x.Source == source &&
                 x.Size == size
            ))
            {
                List<byte> data = new List<byte>();
                data.AddRange(msg.CommandData);
                data.AddRange(embeddedConfig.Registers.First(x =>
                    x.Offset == offset &&
                    x.DerefDepth == derefDepth &&
                    x.ReadWrite == readWrite &&
                    x.Source == source &&
                    x.Size == size
                    ).Value.ValueByteArray);
                connector.SendMessage(MessageCodec.EncodeMessage(new ProtocolMessage(msg.ControllerID, msg.MsgID, msg.Command, data.ToArray())));
            }
            else
            {
                connector.SendMessage(MessageCodec.EncodeMessage(new ProtocolMessage(msg.ControllerID, msg.MsgID, msg.Command, msg.CommandData)));
            }
        }

        private void DispatchEmbeddedConfigurationMessage(ProtocolMessage msg)
        {
            if (msg.CommandData.Length == 0)
            {
                foreach (ProtocolMessage config in TemplateProvider.GetConfiguration(msg.MsgID, embeddedConfig, msg.ControllerID))
                {
                    connector.SendMessage(MessageCodec.EncodeMessage(config));
                }
            }
            else if (msg.CommandData.Length == 4)
            {
                foreach (ProtocolMessage config in TemplateProvider.GetConfiguration(msg.MsgID, embeddedConfig, msg.ControllerID, BitConverter.ToUInt32(msg.CommandData, 0)))
                {
                    connector.SendMessage(MessageCodec.EncodeMessage(config));
                }
            }
        }

        private void DispatchReadChannelDataMessage(ProtocolMessage msg)
        {
            if (msg.CommandData.Length != 1)
            {
                connector.SendMessage(MessageCodec.EncodeMessage(new ProtocolMessage(msg.ControllerID, msg.MsgID, msg.Command)));
                return;
            }
            if (msg.CommandData[0] == 0x00)
            {
                timer.Stop();
                connector.SendMessage(MessageCodec.EncodeMessage(TemplateProvider.GetAccMsg(msg)));
            }
            else if (msg.CommandData[0] == 0x01)
            {
                timer.Start();
                connector.SendMessage(MessageCodec.EncodeMessage(TemplateProvider.GetAccMsg(msg)));
            }
            else if (msg.CommandData[0] == 0x02)
            {
                List<byte> data = new List<byte>
                {
                    (byte)(stopwatch.ElapsedMilliseconds),
                    (byte)(stopwatch.ElapsedMilliseconds >> 8),
                    (byte)(stopwatch.ElapsedMilliseconds >> 16)
                };
                ushort mask = 0b0000_0000_0000_0000;
                foreach (Register r in embeddedConfig.Registers.Where(x => x.IsDebugChannel && x.ChannelMode == ChannelMode.Once).OrderBy(x => x.DebugChannel))
                {
                    mask = (ushort)(mask | 1 << r.DebugChannel);
                    data.AddRange(r.Value.ValueByteArray);
                }
                data.Insert(3, (byte)mask);
                data.Insert(4, (byte)(mask >> 8));
                connector.SendMessage(MessageCodec.EncodeMessage(new ProtocolMessage(msg.ControllerID, msg.MsgID, msg.Command, data.ToArray())));
            }
            else
            {
                connector.SendMessage(MessageCodec.EncodeMessage(TemplateProvider.GetAccMsg(msg)));
            }
        }

        private void DispatchDebugStringMessage(ProtocolMessage msg)
        {
            string msgString = Encoding.UTF8.GetString(msg.CommandData);
            NewDebugString(this, msgString);
        }

        private void DispatchResetTimeMessage(ProtocolMessage msg)
        {
            stopwatch.Restart();
        }

        private void SendTrace(EmbeddedDebugger.DebugProtocol.Enums.TraceLevel level, string message)
        {
            connector.SendMessage(MessageCodec.EncodeMessage(TemplateProvider.GetTraceMessage(new TraceMessage() { TraceLevel = level, Message = message })));
        }

        private void SendTrace(EmbeddedDebugger.DebugProtocol.Enums.TraceLevel level, string message, byte nodeID)
        {
            connector.SendMessage(MessageCodec.EncodeMessage(TemplateProvider.GetTraceMessage(new TraceMessage() { TraceLevel = level, Message = message, NodeID = nodeID })));
        }
    }
}
