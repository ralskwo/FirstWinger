﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Reflection;
using System.Text;

public class MarshalTableConstant
{
    public const int charBufferSize = 256;
}


public class TableRecordParser<TMarshalStruct>
{
    public TMarshalStruct PaeseRecordLine(string line)
    {
        // TMarshalStruct 크기에 맞춰서 Byte 배열 할당
        Type type = typeof(TMarshalStruct);
        int structSize = Marshal.SizeOf(type);                      // using System.Runtime.InteropServices.Marshal
        byte[] structBytes = new byte[structSize];
        int structBytesIndex = 0;

        // line 문자열을 spliter로 자름
        const string spliter = ",";
        string[] fieldDataList = line.Split(spliter.ToCharArray());

        // 타입을 보고 바이너리에 파싱하여 삽입
        Type dataType;
        string splited;
        byte[] fieldByte;
        //byte[] keyBytes;

        FieldInfo[] fieldInfos = type.GetFields();                   // using System.Reflection.FieldInfo

        for (int i = 0; i < fieldInfos.Length; i++)
        {
            dataType = fieldInfos[i].FieldType;
            splited = fieldDataList[i];

            fieldByte = new byte[4];
            MakeBytesByFieldType(out fieldByte, dataType, splited);

            Buffer.BlockCopy(fieldByte, 0, structBytes, structBytesIndex, fieldByte.Length);
            structBytesIndex += fieldByte.Length;

            // 첫 번째 필드를 Key값으로 사용하기 위해 백업
            // if (i == 0)
            //     keyBytes = fieldByte;
        }

        // 마샬링
        TMarshalStruct tStruct = MakeStructFromBytes<TMarshalStruct>(structBytes);
        return tStruct;
    }


    protected void MakeBytesByFieldType(out byte[] fieldByte, Type dataType, string splite)
    {
        fieldByte = new byte[1];
        if (typeof(int) == dataType)
        {
            fieldByte = BitConverter.GetBytes(int.Parse(splite));       // using System.BitConverter
        }

        else if (typeof(float) == dataType)
        {
            fieldByte = BitConverter.GetBytes(float.Parse(splite));
        }

        else if (typeof(bool) == dataType)
        {
            bool value = bool.Parse(splite);
            int temp = value ? 1 : 0;

            fieldByte = BitConverter.GetBytes((int)temp);
        }

        else if (typeof(string) == dataType)
        {
            // 마샬링을 위한 고정크기 버퍼 생성
            fieldByte = new byte[MarshalTableConstant.charBufferSize];
            byte[] byteArr = Encoding.UTF8.GetBytes(splite);                    // System.Text.Encoding
            // 변환된 byte배열을 고정크기 버퍼에 복사
            Buffer.BlockCopy(byteArr, 0, fieldByte, 0, byteArr.Length);         // System.Buffer
        }
    }

    public static T MakeStructFromBytes<T>(byte[] bytes)
    {
        int size = Marshal.SizeOf(typeof(T));
        IntPtr ptr = Marshal.AllocHGlobal(size);            // 마샬 메모리 할당

        Marshal.Copy(bytes, 0, ptr, size);                  // 복사

        T tStruct = (T)Marshal.PtrToStructure(ptr, typeof(T));          // 메모리로부터 T형 구조체로 변환
        Marshal.FreeHGlobal(ptr);           // 할당된 메모리 해제

        return tStruct;
    }
}
