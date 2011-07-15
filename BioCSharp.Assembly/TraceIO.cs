using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using MathLib;

namespace BioCSharp.Assembly
{
    public static class TraceIO
    {

        

        /// <summary>
        /// reads files in SCF trace format.
        /// </summary>
        /// <param name="fileName"></param>
        public static void ScfRead(string fileName, out int[] A, out int[] C, out int[] G, out int[] T,
            out int[] probA, out int[] probC, out int[] probG, out int[] probT,
            out int[] peakIndex, out char[] bases)
        {
            char fmt = 'B';

            if (!File.Exists(fileName))
                if (File.Exists(fileName + ".scf")) fileName += ".scf";
            if (!File.Exists(fileName))
                throw new IOException("File not found: " + fileName);
            FileStream fid = new FileStream(fileName, FileMode.Open);


            long magic_number = readUInt32(fid, fmt);
            long num_samples = readUInt32(fid, fmt);
            long samples_offset = readUInt32(fid, fmt);
            long num_bases = readUInt32(fid, fmt);           // Number of bases in Bases matrix
            long bases_left_clip = readUInt32(fid, fmt);      // OBSOLETE: No. bases in left clip (vector)
            long bases_right_clip = readUInt32(fid, fmt);    //%#ok OBSOLETE: No. bases in right clip (qual)
            long bases_offset = readUInt32(fid, fmt);        //% Byte offset from start of file
            long comments_size = readUInt32(fid, fmt);       //% Number of bytes in Comment section
            long comments_offset = readUInt32(fid, fmt);
            long version = readUInt32(fid, fmt);           //   %#ok "version.revision", eg '3' '.' '0' '0'
            long sample_size = readUInt32(fid, fmt);        // % Size of samples in bytes 1=8bits, 2=16bits
            if (sample_size != 1 && sample_size != 2)
            {
                // probabily little/big endian problem
                fmt = 'L';
                fid.Seek(0, SeekOrigin.Begin);

                magic_number = readUInt32(fid, fmt);
                num_samples = readUInt32(fid, fmt);
                samples_offset = readUInt32(fid, fmt);
                num_bases = readUInt32(fid, fmt);           // Number of bases in Bases matrix
                bases_left_clip = readUInt32(fid, fmt);      // OBSOLETE: No. bases in left clip (vector)
                bases_right_clip = readUInt32(fid, fmt);    //%#ok OBSOLETE: No. bases in right clip (qual)
                bases_offset = readUInt32(fid, fmt);        //% Byte offset from start of file
                comments_size = readUInt32(fid, fmt);       //% Number of bytes in Comment section
                comments_offset = readUInt32(fid, fmt);
                version = readUInt32(fid, fmt);           //   %#ok "version.revision", eg '3' '.' '0' '0'
                sample_size = readUInt32(fid, fmt);        // % Size of samples in bytes 1=8bits, 2=16bits
            }
            long code_set = readUInt32(fid, fmt);           // %#ok code set used (but ignored!)
            long private_size = readUInt32(fid, fmt);       // %#ok No. of bytes of Private data, 0 if none
            long private_offset = readUInt32(fid, fmt);     // %#ok Byte offset from start of file
            long spare = readUInt32(fid, fmt);

            if (sample_size != 1 && sample_size != 2)
            {
                fid.Close();
                throw new NotSupportedException("Problems reading the file. Sample size is not 1 or 2.'");
            }



            // move cursor to beginning of trace information
            fid.Seek(samples_offset, SeekOrigin.Begin);

            string type = "int16";
            if (sample_size == 1)
                type = "int8";

            A = readArray(fid, num_samples, type, fmt);
            C = readArray(fid, num_samples, type, fmt);
            G = readArray(fid, num_samples, type, fmt);
            T = readArray(fid, num_samples, type, fmt);

            A = General.CumSum(General.CumSum(A));
            C = General.CumSum(General.CumSum(C));
            G = General.CumSum(General.CumSum(G));
            T = General.CumSum(General.CumSum(T));

            fid.Seek(bases_offset, SeekOrigin.Begin);

            //  %  Index into Samples matrix for base posn
            peakIndex = readArray(fid, num_bases, "uint32", fmt);
            probA = readArray(fid, num_bases, "uint8", fmt);     //        %  Probability of it being an A
            probC = readArray(fid, num_bases, "uint8", fmt);       //      %  Probability of it being an C
            probG = readArray(fid, num_bases, "uint8", fmt);         //    %  Probability of it being an G
            probT = readArray(fid, num_bases, "uint8", fmt);           //  %  Probability of it being an T
            bases = new char[num_bases];
            for (int i = 0; i < num_bases; ++i)
            {
                bases[i] = Convert.ToChar(readInt8(fid));
            }

            Console.WriteLine(bases[num_bases - 3]);
            Console.WriteLine(bases[num_bases - 2]);
            Console.WriteLine(bases[num_bases - 1]);

            fid.Close();
        }

        public static void ScfRead(string fileName, out char[] bases)
        {
            int[] A; int[] C; int[] G; int[] T;
            int[] probA; int[] probC; int[] probG; int[] probT;
            int[] peakIndex;
            ScfRead(fileName, out A, out  C, out  G, out T,
                out probA, out probC, out probG, out probT, out peakIndex, out bases);
        }


        private static int[] readArray(FileStream fid, long num_samples, string type, char fmt)
        {
            int[] A = new int[num_samples];
            for (long i = 0; i < num_samples; ++i)
            {
                if ("int8" == type)
                    A[i] = readInt8(fid);
                else if ("int16" == type)
                    A[i] = readInt16(fid, fmt);
                else if ("uint8" == type)
                    A[i] = readUInt8(fid);
                else if ("uint32" == type)
                    A[i] = (int)readUInt32(fid, fmt);
                else
                    throw new NotSupportedException();
            }
            return A;
        }

        private static long readUInt32(FileStream fid, char fmt)
        {
            byte[] buffer = new byte[4];
            long offset = fid.Read(buffer, 0, 4);
            if (fmt == 'B')
            {
                byte[] bigEndian = new byte[4] { buffer[3], buffer[2], buffer[1], buffer[0] };
                return (long)System.BitConverter.ToUInt32(bigEndian, 0);
            }
            else
            {
                return (long)System.BitConverter.ToUInt32(buffer, 0);
            }
        }

        private static int readInt16(FileStream fid, char fmt)
        {
            byte[] buffer = new byte[2];
            long offset = fid.Read(buffer, 0, 2);
            if (fmt == 'B')
            {
                byte[] bigEndian = new byte[2] { buffer[1], buffer[0] };
                return (int)System.BitConverter.ToInt16(bigEndian, 0);
            }
            else
            {
                return System.BitConverter.ToInt16(buffer, 0);
            }
        }


        private static int readInt8(FileStream fid)
        {
            byte[] buffer = new byte[1];
            int offset = fid.Read(buffer, 0, 1);
            return (int)buffer[0];
        }

        private static int readUInt8(FileStream fid)
        {
            byte[] buffer = new byte[1];
            int offset = fid.Read(buffer, 0, 1);
            return (int)buffer[0];
        }

       
    }
        
    
}

