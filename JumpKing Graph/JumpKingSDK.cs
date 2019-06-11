using System;
using System.Diagnostics;

namespace JumpKing_Graph {


    class JumpKingSDK {
        private Process Process = null;
        private IntPtr CamScreenPtr;

        public bool FoundOffsets() {
            return this.CamScreenPtr != IntPtr.Zero;
        }
        public bool HasProcess() {
            return this.Process != null && !this.Process.HasExited;
        }
        public bool IsReady() {
            return this.FoundOffsets() && !this.HasProcess();
        }

        public void Init() {
            Process[] JumpKingProc = Process.GetProcessesByName("JumpKing");

            if(JumpKingProc.Length != 1)
                throw new Exception("JumpKing is not running right now");

            this.Process = JumpKingProc[0];

            this.CamScreenPtr = MemHax.SigScan(this.Process, null, new byte[] { 0x5E, 0x5F, 0x5D, 0xC2, 0x00, 0x00, 0x89, 0x3D, 0x00, 0x00, 0x00, 0x00, 0x8B, 0x15 }, 8);
        }

        public ushort GetCurrentScreen() {
            if(!this.IsReady())
                return 0;

            byte[] we = new byte[2];

            MemHax.ReadProcessMemory(this.Process.Handle, this.CamScreenPtr, we, 2);

            ushort toRet = BitConverter.ToUInt16(we, 0);

            return toRet++;
        }

        public ushort IsStanding() {
            return 1;
        }

    }
}
