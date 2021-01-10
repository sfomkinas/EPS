using System.IO;

namespace EPS.Protocol
{
    public enum ActionEnum
    {
        GenerateCodes = 1,
        UseCode = 2,
        CheckCode = 3
    }

    public enum CodeGenerateEnum
    {
        Generated = 1,
        Error = 2,
    }

    public enum UseCodeEnum
    {
        Used = 1,
        AlreadyUsed = 2,
        Error = 3,
        NoCode = 4,
    }

    public struct HeaderRequest
    {
        public ActionEnum Action;

        public void Read(Stream s)
        {
            BinaryReader R = new BinaryReader(s);
            Action = (ActionEnum)R.ReadInt32();
        }

        public void Write(Stream s)
        {
            BinaryWriter w = new BinaryWriter(s);
            w.Write((int)Action);
        }
    }

    public struct HeaderResponse
    {
        public ActionEnum Action;

        public void Read(Stream s)
        {
            BinaryReader R = new BinaryReader(s);
            Action = (ActionEnum)R.ReadInt32();
        }

        public void Write(Stream s)
        {
            BinaryWriter w = new BinaryWriter(s);
            w.Write((int)Action);
        }
    }

    public struct GenereteCodesRequest
    {
        public int Length;
        public int Count;

        public void Read(Stream s)
        {
            BinaryReader R = new BinaryReader(s);
            Length = R.ReadInt32();
            Count = R.ReadInt32();
        }

        public void Write(Stream s)
        {
            BinaryWriter w = new BinaryWriter(s);
            w.Write((int)Length);
            w.Write((int)Count);
        }
    }

    public struct GenereteCodesResponse
    {
        public CodeGenerateEnum CodeGenerate;
        public string Notification;

        public void Read(Stream s)
        {
            BinaryReader R = new BinaryReader(s);
            CodeGenerate = (CodeGenerateEnum)R.ReadInt32();
            Notification = R.ReadString();
        }

        public void Write(Stream s)
        {
            BinaryWriter w = new BinaryWriter(s);
            w.Write((int)CodeGenerate);
            w.Write(Notification);
        }
    }

    public struct UseCodeRequest
    {
        public string Code;

        public void Read(Stream s)
        {
            BinaryReader R = new BinaryReader(s);
            Code = R.ReadString();
        }

        public void Write(Stream s)
        {
            BinaryWriter w = new BinaryWriter(s);
            w.Write(Code);
        }
    }

    public struct UseCodeResponse
    {
        public UseCodeEnum Result;
        public string Notification;

        public void Read(Stream s)
        {
            BinaryReader R = new BinaryReader(s);
            Result = (UseCodeEnum)R.ReadInt32();
            Notification = R.ReadString();
        }

        public void Write(Stream s)
        {
            BinaryWriter w = new BinaryWriter(s);
            w.Write((int)Result);
            w.Write(Notification);
        }
    }
}
