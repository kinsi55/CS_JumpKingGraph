using System;
using System.Runtime.InteropServices;
using System.Diagnostics;

namespace JumpKing_Graph {

    internal class MemHax {
        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool ReadProcessMemory(
            IntPtr hProcess,
            IntPtr lpBaseAddress,
            [Out()] byte[] lpBuffer,
            int dwSize,
            int lpNumberOfBytesRead = 0
        );

        [DllImport("kernel32.dll")]
        static extern int VirtualQueryEx(
            IntPtr hProcess,
            IntPtr lpAddress,
            out MEMORY_BASIC_INFORMATION lpBuffer,
            uint dwLength);

        [StructLayout(LayoutKind.Sequential)]
        public struct MEMORY_BASIC_INFORMATION {
            public IntPtr BaseAddress;
            public IntPtr AllocationBase;
            public uint AllocationProtect;
            public IntPtr RegionSize;
            public uint State;
            public uint Protect;
            public uint Type;
        }
        public enum AllocationProtect : uint {
            PAGE_EXECUTE = 0x00000010,
            PAGE_EXECUTE_READ = 0x00000020,
            PAGE_EXECUTE_READWRITE = 0x00000040,
            PAGE_EXECUTE_WRITECOPY = 0x00000080,
            PAGE_NOACCESS = 0x00000001,
            PAGE_READONLY = 0x00000002,
            PAGE_READWRITE = 0x00000004,
            PAGE_WRITECOPY = 0x00000008,
            PAGE_GUARD = 0x00000100,
            PAGE_NOCACHE = 0x00000200,
            PAGE_WRITECOMBINE = 0x00000400
        }

        public static IntPtr SigScan(Process proc, ProcessModule Module, byte[] sig, params int[] offsets) {
            int MaxAddress = 0x7fffffff;
            int address = 0;

            if(Module != null) {
                address = (int)Module.BaseAddress;
                MaxAddress = (int)Module.ModuleMemorySize;
            }

            byte[] memreg = null;

            do {
                MEMORY_BASIC_INFORMATION m;
                int result = VirtualQueryEx(proc.Handle, (IntPtr)address, out m, (uint)Marshal.SizeOf(typeof(MEMORY_BASIC_INFORMATION)));

                if(memreg == null || memreg.Length != (int)m.RegionSize)
                    memreg = new byte[(int)m.RegionSize];

                if(ReadProcessMemory(proc.Handle, m.BaseAddress, memreg, memreg.Length)) {
                    for(int subPos = 0; subPos < memreg.Length - sig.Length; subPos++) {
                        for(int i = 0; i < sig.Length; i++) {
                            if(sig[i] != 0x00 && sig[i] != memreg[subPos + i])
                                break;

                            if(i != sig.Length - 1)
                                continue;

                            if(offsets.Length == 0)
                                return new IntPtr((int)m.BaseAddress + subPos);

                            return Dereferer(proc, memreg, (int)m.BaseAddress, subPos, offsets);
                        }
                    }
                }

                if(address == (int)m.BaseAddress + (int)m.RegionSize || Module != null)
                    break;
                address = (int)m.BaseAddress + (int)m.RegionSize;
            } while(address <= MaxAddress);

            return IntPtr.Zero;
        }

        public static IntPtr Dereferer(Process proc, byte[] memreg, int scanPos, int subPos, int[] offsets) {
            if(offsets.Length == 0)
                throw new MissingFieldException("Must pass at least one offset to deref");

            IntPtr retVal = IntPtr.Zero;

            for(int i = 0; i < offsets.Length; i++) {
                int offs = offsets[i];

                if((offs > 0 && memreg.Length - subPos - 4 > offs) || (offs < 0 && subPos >= -offs))
                    retVal = new IntPtr(BitConverter.ToInt32(memreg, subPos + offs));
                else if(!ReadProcessMemory(proc.Handle, new IntPtr(scanPos + subPos + offs), memreg, 4))
                    return IntPtr.Zero;
                else
                    retVal = new IntPtr(BitConverter.ToInt32(memreg, 0));

                int newPos = (int)retVal;
                subPos = 0;

                if(i != offsets.Length - 1 && (newPos < scanPos || newPos > scanPos + memreg.Length - 4)) {
                    offs = offsets[i + 1];

                    if(!ReadProcessMemory(proc.Handle, retVal + offs, memreg, 4))
                        return IntPtr.Zero;

                    scanPos = newPos;
                }

            }

            return retVal;
        }
    }
}
