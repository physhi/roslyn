﻿// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Roslyn.Utilities
{
    internal struct ObjectBinderState
    {
        private readonly Dictionary<Type, int> _typeToIndex;
        private readonly List<Type> _types;
        private readonly List<Func<ObjectReader, object>> _typeReaders;

        private ObjectBinderState(
            Dictionary<Type, int> typeToIndex,
            List<Type> types,
            List<Func<ObjectReader, object>> typeReaders)
        {
            _typeToIndex = typeToIndex;
            _types = types;
            _typeReaders = typeReaders;
        }

        public static ObjectBinderState Create()
            => new ObjectBinderState(new Dictionary<Type, int>(), new List<Type>(), new List<Func<ObjectReader, object>>());

        public void Clear()
        {
            _typeToIndex.Clear();
            _types.Clear();
            _typeReaders.Clear();
        }

        public void CopyFrom(ObjectBinderState other)
        {
            Debug.Assert(_typeToIndex.Count == 0);
            Debug.Assert(_types.Count == 0);
            Debug.Assert(_typeReaders.Count == 0);

            foreach (var kvp in other._typeToIndex)
            {
                _typeToIndex.Add(kvp.Key, kvp.Value);
            }

            _types.AddRange(other._types);
            _typeReaders.AddRange(other._typeReaders);
        }

        public int GetTypeId(Type type)
            => _typeToIndex[type];

        public int GetOrAddTypeId(Type type)
        {
            if (!_typeToIndex.TryGetValue(type, out var index))
            {
                RegisterTypeReader(type, typeReader: null);
                index = _typeToIndex[type];
            }

            return index;
        }

        public Type GetTypeFromId(int typeId)
            => _types[typeId];

        public (Type, Func<ObjectReader, object>) GetTypeAndReaderFromId(int typeId)
            => (_types[typeId], _typeReaders[typeId]);

        public Func<ObjectReader, object> GetTypeReader(int index)
            => _typeReaders[index];

        public void RegisterTypeReader(Type type, Func<ObjectReader, object> typeReader)
        {
            if (!_typeToIndex.ContainsKey(type))
            {
                int index = _typeReaders.Count;
                _types.Add(type);
                _typeReaders.Add(typeReader);
                _typeToIndex.Add(type, index);

                // We may be a local copy of the object-binder-state.  Inform the primary 
                // binder of this new registration.  Note: there is no re-entrancy issue here
                // as we've already updated our local state, so we'll bail out immediately
                // when we get called back through RegisterTypeReader.
                ObjectBinder.RegisterTypeReader(type, typeReader);
            }
        }
    }
}