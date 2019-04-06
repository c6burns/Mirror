#define MIRROR_BUFFER_STACK_DEBUG

using System;
using System.Collections.Generic;
using UnityEngine;

namespace Mirror.Buffers
{
    public static class BufferManager
    {
        #region AquiredRef and AquiredRef internal classes
        internal class AcquiredRef<T>
        {
            readonly T _classRef;
#if MIRROR_BUFFER_STACK_DEBUG
            readonly string _allocStack;
            readonly DateTime _allocTime;
#endif

            public AcquiredRef(T classInstance)
            {
                _classRef = classInstance;
#if MIRROR_BUFFER_STACK_DEBUG
                _allocStack = Environment.StackTrace;
                _allocTime = DateTime.Now;
#endif
            }

            public T Reference()
            {
                return _classRef;
            }

#if MIRROR_BUFFER_STACK_DEBUG
            public string StackTrace()
            {
                return _allocStack;
            }

            public DateTime AllocTime()
            {
                return _allocTime;
            }
#endif
        }

        internal class AcquiredRefList<T>
        {
            Dictionary<T, AcquiredRef<T>> _refMap = new Dictionary<T, AcquiredRef<T>>();
            Stack<AcquiredRef<T>> _refFreeStack = new Stack<AcquiredRef<T>>();
            public T Acquire()
            {
                if (_refFreeStack.Count > 0)
                {
                    return _refFreeStack.Pop().Reference();

                }

                return default;
            }

            public void Add(T classRef)
            {
                _refMap.Add(classRef, new AcquiredRef<T>(classRef));
            }

            public void Release(T classRef)
            {
                if (_refMap.TryGetValue(classRef, out AcquiredRef<T> val))
                {
                    _refFreeStack.Push(val);
                }
            }

            public bool HasLeakedReferences()
            {
                return (_refFreeStack.Count == _refMap.Count);
            }

            public void DebugReferences()
            {
#if MIRROR_BUFFER_STACK_DEBUG
                while (_refFreeStack.Count > 0)
                {
                    AcquiredRef<T> classRef = _refFreeStack.Pop();
                    if (!_refMap.Remove(classRef.Reference()))
                    {
                        Debug.LogErrorFormat("Not in map: {0}\n{1}", classRef.AllocTime().ToShortTimeString(), classRef.StackTrace());
                    }

                    foreach (KeyValuePair<T, AcquiredRef<T>> entry in _refMap)
                    {
                        Debug.LogWarningFormat("Not released: {0}\n{1}", entry.Value.AllocTime().ToShortTimeString(), entry.Value.StackTrace());
                    }
                }
#else
                Debug.LogWarning("Reference details not collected without MIRROR_ALLOC_STACK_DEBUG defined");
#endif
            }
        }
        #endregion


        static BufferAllocator _defaultBufferAllocator = new BufferAllocator();
        static IBufferAllocator _bufferAllocator = _defaultBufferAllocator;
        static AcquiredRefList<NetworkWriter> _writerList = new AcquiredRefList<NetworkWriter>();
        static AcquiredRefList<NetworkReader> _readerList = new AcquiredRefList<NetworkReader>();

        static BufferManager()
        {
            // cctor aka static ctor
        }

        public static void RegisterAllocator(IBufferAllocator allocator)
        {
            _bufferAllocator = allocator;
        }

        public static void UnregisterAllocator(IBufferAllocator allocator)
        {
            if (_bufferAllocator == allocator)
            {
                _bufferAllocator = _defaultBufferAllocator;
            }
        }

        public static IBuffer AcquireBuffer(ulong minSizeInBytes)
        {
            return _bufferAllocator.Acquire(minSizeInBytes);
        }

        public static IBuffer ReacquireBuffer(IBuffer rentedBuffer, ulong newMinSizeInBytes)
        {
            return _bufferAllocator.Reacquire(rentedBuffer, newMinSizeInBytes);
        }

        public static void ReleaseBuffer(IBuffer rentedBuffer)
        {
            _bufferAllocator.Release(rentedBuffer);
        }

        public static NetworkWriter AcquireWriter()
        {
            NetworkWriter writer = _writerList.Acquire();
            if (writer == null)
            {
                writer = new NetworkWriter();
                _writerList.Add(writer);
            }
            return writer;
        }

        public static void ReleaseWriter(NetworkWriter writer)
        {
            _writerList.Release(writer);
        }

        public static NetworkReader AcquireReader(byte[] buffer)
        {
            NetworkReader reader = _readerList.Acquire();
            if (reader == null)
            {
                reader = new NetworkReader(buffer);
                _readerList.Add(reader);
            }
            else
            {
                // TODO: insert buffer into reader
            }
            return reader;
        }

        public static void ReleaseReader(NetworkReader reader)
        {
            _readerList.Release(reader);
        }
    }
}
