﻿using System;
using System.Collections.Generic;
using System.Text;

namespace DogScepterLib.Core.Models
{
    class GMFunctionEntry : GMSerializable
    {
        public GMString Name;
        public int StringIndex; // Index of the string in the STRG chunk, appears to be unused(?)
        public uint Occurances;
        public uint FirstAddress; // TODO: Should be a bytecode instruction

        public void Serialize(GMDataWriter writer)
        {
            writer.WritePointerString(Name);
            writer.Write(Occurances);
            if (Occurances > 0)
            {
                if (writer.VersionInfo.IsNumberAtLeast(2, 3))
                {
                    writer.Write(FirstAddress + 4);
                }
                else
                {
                    writer.Write(FirstAddress);
                }
            }
            else
                writer.Write((int)-1);
        }

        public void Unserialize(GMDataReader reader)
        {
            Name = reader.ReadStringPointerObject();
            Occurances = reader.ReadUInt32();
            if (Occurances > 0)
            {
                if (reader.VersionInfo.IsNumberAtLeast(2, 3))
                {
                    FirstAddress = reader.ReadUInt32() - 4;
                }
                else
                {
                    FirstAddress = reader.ReadUInt32();
                }
            }
            else
            {
                if (reader.ReadInt32() != -1)
                    reader.Warnings.Add(new GMWarning("Function with no occurrences, but still has a first occurrence address"));
                FirstAddress = 0;
            }
        }
    }
}
