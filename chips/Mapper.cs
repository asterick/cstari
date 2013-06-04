using System;

namespace cstari.chips
{
    /// <summary>
    /// Prototype interface for mappers
    /// </summary>

    abstract public class Mapper
    {
        abstract public byte access(ushort address, byte data);
        abstract public byte read(ushort address);

        virtual public void clock(int cycles) { }

        abstract public int getBlockCount();
        abstract public string getBlockName(int block);
        abstract public int getBlockLength(int block);
        abstract public byte getBlockData(int block, ushort address);
    }
}
