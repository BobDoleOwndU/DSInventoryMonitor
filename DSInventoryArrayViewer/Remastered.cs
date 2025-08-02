using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using static DSInventoryMonitor.MemoryHandler;

namespace DSInventoryMonitor
{
    public static class Remastered
    {
        public static string WindowTitle { get; set; } = "DARK SOULS™: REMASTERED";
        public static Process process;
        private static string AOB { get; set; } = "48 8B 05 ? ? ? ? 45 33 ED 48 8B F1 48 85 C0";

        private static IntPtr GetCharDataAddress()
        {
            if (process == null)
            {
                throw new Exception("Failed to get Dark Souls Remastered process!");
            } //if

            IntPtr processHandle = OpenProcess(ProcessAccessFlags.All, false, process.Id);
            IntPtr moduleAddress = IntPtr.Zero;
            long moduleSize = 0;

            foreach (ProcessModule m in process.Modules)
            {
                if (m.ModuleName == "DarkSoulsRemastered.exe")
                {
                    moduleAddress = m.BaseAddress;
                    moduleSize = m.ModuleMemorySize;
                    break;
                } //if
            } //foreach

            IntPtr charDataAddress = IntPtr.Zero;

            long searchStart = (long)moduleAddress;
            long searchEnd = searchStart + moduleSize;

            IntPtr address = (IntPtr)searchStart;
            IntPtr bytesRead;
            byte[] buffer;

            string searchString = AOB;
            byte[] searchBytes;
            string[] searchMask;

            SearchStringToValues(searchString, out searchBytes, out searchMask);

            do
            {
                MEMORY_BASIC_INFORMATION64 m;
                int result = VirtualQueryEx(processHandle, address, out m, (uint)Marshal.SizeOf(typeof(MEMORY_BASIC_INFORMATION64)));

                buffer = new byte[m.RegionSize];
                ReadProcessMemory(processHandle, address, buffer, buffer.Length, out bytesRead);
                bool found = false;

                for (int i = 0; i < buffer.Length - searchBytes.Length; i++)
                {
                    found = true;

                    for (int j = 0; j < searchBytes.Length; j++)
                    {
                        if (searchMask[j] != "?")
                        {
                            if (searchBytes[j] != buffer[i + j])
                            {
                                found = false;
                                break;
                            } //if
                        } //if
                    } //for

                    if (found)
                    {
                        charDataAddress = address + i;
                        break;
                    } //if
                } //for

                if (found)
                    break;

                address = (IntPtr)((ulong)address + m.RegionSize);
            } //do
            while ((long)address < searchEnd);

            buffer = new byte[4];

            ReadProcessMemory(processHandle, charDataAddress + 3, buffer, buffer.Length, out bytesRead);
            charDataAddress = (IntPtr)(charDataAddress + BitConverter.ToInt32(buffer, 0) + 7);
            ReadProcessMemory(processHandle, charDataAddress, buffer, buffer.Length, out bytesRead);
            charDataAddress = (IntPtr)BitConverter.ToInt32(buffer, 0);
            ReadProcessMemory(processHandle, charDataAddress + 0x10, buffer, buffer.Length, out bytesRead);
            charDataAddress = (IntPtr)BitConverter.ToInt32(buffer, 0);

            CloseHandle(processHandle);

            Debug.WriteLine(charDataAddress.ToString("x"));

            return charDataAddress;
        } //ReadMemory

        public static InventorySlot[] ReadInventoryArray()
        {
            InventorySlot[] inventorySlots;

            IntPtr charDataAddress = GetCharDataAddress();

            if (process == null)
            {
                throw new Exception("Failed to get Dark Souls Remastered process!");
            } //if

            IntPtr processHandle = OpenProcess(ProcessAccessFlags.All, false, process.Id);
            byte[] buffer;
            IntPtr bytesRead;

            int offset = 0x670;
            int arraySize;

            buffer = new byte[4];
            ReadProcessMemory(processHandle, charDataAddress + offset, buffer, buffer.Length, out bytesRead);

            offset += 8;
            ReadProcessMemory(processHandle, charDataAddress + offset, buffer, buffer.Length, out bytesRead);
            arraySize = BitConverter.ToInt32(buffer, 0);
            inventorySlots = new InventorySlot[arraySize];

            for (int i = 0; i < arraySize; i++)
            {
                inventorySlots[i] = new InventorySlot();

                offset = 0x680 + i * 0x1C;

                ReadProcessMemory(processHandle, charDataAddress + offset, buffer, buffer.Length, out bytesRead);
                inventorySlots[i].itemType = BitConverter.ToInt32(buffer, 0);
                offset += 4;

                ReadProcessMemory(processHandle, charDataAddress + offset, buffer, buffer.Length, out bytesRead);
                inventorySlots[i].id = BitConverter.ToInt32(buffer, 0);
                offset += 4;

                ReadProcessMemory(processHandle, charDataAddress + offset, buffer, buffer.Length, out bytesRead);
                inventorySlots[i].quantity = BitConverter.ToInt32(buffer, 0);
                offset += 4;

                buffer = new byte[1];
                ReadProcessMemory(processHandle, charDataAddress + offset, buffer, buffer.Length, out bytesRead);
                inventorySlots[i].idx = buffer[0];
                offset += 1;

                ReadProcessMemory(processHandle, charDataAddress + offset, buffer, buffer.Length, out bytesRead);
                inventorySlots[i].unk1 = buffer[0];
                offset += 1;

                ReadProcessMemory(processHandle, charDataAddress + offset, buffer, buffer.Length, out bytesRead);
                inventorySlots[i].unk2 = buffer[0];
                offset += 1;

                ReadProcessMemory(processHandle, charDataAddress + offset, buffer, buffer.Length, out bytesRead);
                inventorySlots[i].unk3 = buffer[0];
                offset += 1;

                buffer = new byte[4];
                ReadProcessMemory(processHandle, charDataAddress + offset, buffer, buffer.Length, out bytesRead);
                inventorySlots[i].exists = BitConverter.ToInt32(buffer, 0);
                offset += 4;

                ReadProcessMemory(processHandle, charDataAddress + offset, buffer, buffer.Length, out bytesRead);
                inventorySlots[i].unk4 = BitConverter.ToInt32(buffer, 0);
                offset += 4;

                ReadProcessMemory(processHandle, charDataAddress + offset, buffer, buffer.Length, out bytesRead);
                inventorySlots[i].unk5 = BitConverter.ToInt32(buffer, 0);
                offset += 4;
            } //for

            CloseHandle(processHandle);

            return inventorySlots;
        } //ReadInventoryArray

        public static Equipment ReadEquipment()
        {
            Equipment equipment = new Equipment();

            IntPtr charDataAddress = GetCharDataAddress();

            if (process == null)
            {
                throw new Exception("Failed to get Dark Souls Remastered process!");
            } //if

            IntPtr processHandle = OpenProcess(ProcessAccessFlags.All, false, process.Id);
            byte[] buffer;
            IntPtr bytesRead;

            int offset = 0x2A4;

            buffer = new byte[4];
            ReadProcessMemory(processHandle, charDataAddress + offset, buffer, buffer.Length, out bytesRead);
            equipment.leftHand1 = BitConverter.ToInt32(buffer, 0);
            offset += 4;

            ReadProcessMemory(processHandle, charDataAddress + offset, buffer, buffer.Length, out bytesRead);
            equipment.rightHand1 = BitConverter.ToInt32(buffer, 0);
            offset += 4;

            ReadProcessMemory(processHandle, charDataAddress + offset, buffer, buffer.Length, out bytesRead);
            equipment.leftHand2 = BitConverter.ToInt32(buffer, 0);
            offset += 4;

            ReadProcessMemory(processHandle, charDataAddress + offset, buffer, buffer.Length, out bytesRead);
            equipment.rightHand2 = BitConverter.ToInt32(buffer, 0);
            offset += 4;

            ReadProcessMemory(processHandle, charDataAddress + offset, buffer, buffer.Length, out bytesRead);
            equipment.arrow1 = BitConverter.ToInt32(buffer, 0);
            offset += 4;

            ReadProcessMemory(processHandle, charDataAddress + offset, buffer, buffer.Length, out bytesRead);
            equipment.bolt1 = BitConverter.ToInt32(buffer, 0);
            offset += 4;

            ReadProcessMemory(processHandle, charDataAddress + offset, buffer, buffer.Length, out bytesRead);
            equipment.arrow2 = BitConverter.ToInt32(buffer, 0);
            offset += 4;

            ReadProcessMemory(processHandle, charDataAddress + offset, buffer, buffer.Length, out bytesRead);
            equipment.bolt2 = BitConverter.ToInt32(buffer, 0);
            offset += 4;

            ReadProcessMemory(processHandle, charDataAddress + offset, buffer, buffer.Length, out bytesRead);
            equipment.head = BitConverter.ToInt32(buffer, 0);
            offset += 4;

            ReadProcessMemory(processHandle, charDataAddress + offset, buffer, buffer.Length, out bytesRead);
            equipment.chest = BitConverter.ToInt32(buffer, 0);
            offset += 4;

            ReadProcessMemory(processHandle, charDataAddress + offset, buffer, buffer.Length, out bytesRead);
            equipment.hands = BitConverter.ToInt32(buffer, 0);
            offset += 4;

            ReadProcessMemory(processHandle, charDataAddress + offset, buffer, buffer.Length, out bytesRead);
            equipment.legs = BitConverter.ToInt32(buffer, 0);
            offset += 8;

            ReadProcessMemory(processHandle, charDataAddress + offset, buffer, buffer.Length, out bytesRead);
            equipment.ring1 = BitConverter.ToInt32(buffer, 0);
            offset += 4;

            ReadProcessMemory(processHandle, charDataAddress + offset, buffer, buffer.Length, out bytesRead);
            equipment.ring2 = BitConverter.ToInt32(buffer, 0);
            offset += 4;

            ReadProcessMemory(processHandle, charDataAddress + offset, buffer, buffer.Length, out bytesRead);
            equipment.good1 = BitConverter.ToInt32(buffer, 0);
            offset += 4;

            ReadProcessMemory(processHandle, charDataAddress + offset, buffer, buffer.Length, out bytesRead);
            equipment.good2 = BitConverter.ToInt32(buffer, 0);
            offset += 4;

            ReadProcessMemory(processHandle, charDataAddress + offset, buffer, buffer.Length, out bytesRead);
            equipment.good3 = BitConverter.ToInt32(buffer, 0);
            offset += 4;

            ReadProcessMemory(processHandle, charDataAddress + offset, buffer, buffer.Length, out bytesRead);
            equipment.good4 = BitConverter.ToInt32(buffer, 0);
            offset += 4;

            ReadProcessMemory(processHandle, charDataAddress + offset, buffer, buffer.Length, out bytesRead);
            equipment.good5 = BitConverter.ToInt32(buffer, 0);

            CloseHandle(processHandle);

            return equipment;
        } //Equipment
    } //class
} //namespace
