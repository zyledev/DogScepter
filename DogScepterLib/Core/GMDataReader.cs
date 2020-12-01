﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using DogScepterLib.Core.Models;
using DogScepterLib.Core.Util;

namespace DogScepterLib.Core
{
    public class GMDataReader : BufferBinaryReader
    {
        public GMData Data;
        public GMData.GMVersionInfo VersionInfo => Data.VersionInfo;
        public List<GMWarning> Warnings;

        public Dictionary<int, GMSerializable> PointerOffsets;

        public GMDataReader(Stream stream) : base(stream)
        {
            Data = new GMData();
            Warnings = new List<GMWarning>();
            PointerOffsets = new Dictionary<int, GMSerializable>();

            // Parse the root chunk of the file, FORM
            if (ReadChars(4) != "FORM")
                throw new GMException("Root chunk is not \"FORM\"; invalid file.");
            Data.FORM = new GMChunkFORM();
            Data.FORM.Unserialize(this);
        }

        /// <summary>
        /// Returns (a possibly empty) object of the object type, at the specified pointer address
        /// </summary>
        public T ReadPointer<T>(int ptr) where T : GMSerializable, new()
        {
            if (ptr == 0)
                return default(T);
            if (PointerOffsets.ContainsKey(ptr))
                return (T)PointerOffsets[ptr];
            T res = new T();
            PointerOffsets[ptr] = res;
            return res;
        }

        /// <summary>
        /// Returns (a possibly empty) object of the object type, at the pointer in the file
        /// </summary>
        public T ReadPointer<T>() where T : GMSerializable, new()
        {
            return ReadPointer<T>(ReadInt32());
        }

        /// <summary>
        /// Follows the specified pointer for an object type, unserializes it and returns it
        /// </summary>
        public T ReadPointerObject<T>(int ptr) where T : GMSerializable, new()
        {
            if (ptr == 0)
                return default(T);

            T res;
            if (PointerOffsets.ContainsKey(ptr))
                res = (T)PointerOffsets[ptr];
            else
            {
                res = new T();
                PointerOffsets[ptr] = res;
            }

            int before = Offset;
            Offset = ptr;

            res.Unserialize(this);

            Offset = before;

            return res;
        }

        /// <summary>
        /// Follows a pointer (in the file) for an object type, unserializes it and returns it
        /// </summary>
        public T ReadPointerObject<T>() where T : GMSerializable, new()
        {
            return ReadPointerObject<T>(ReadInt32());
        }

        /// <summary>
        /// Reads a string without parsing it
        /// </summary>
        public GMString ReadStringPointer()
        {
            return ReadPointer<GMString>(ReadInt32() - 4);
        }

        /// <summary>
        /// Reads a string AND parses it
        /// </summary>
        public GMString ReadStringPointerObject()
        {
            return ReadPointerObject<GMString>(ReadInt32() - 4);
        }

        /// <summary>
        /// Reads a GameMaker-style string
        /// </summary>
        public string ReadGMString()
        {
            int length = ReadInt32();
            string res = Encoding.GetString(Buffer, Offset, length);
            Offset += length;
            if (Buffer[Offset++] != 0)
                Warnings.Add(new GMWarning("String not null terminated around " + Offset.ToString()));
            return res;
        }
    }
}
