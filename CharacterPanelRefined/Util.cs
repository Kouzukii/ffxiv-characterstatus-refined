using System;
using System.Runtime.InteropServices;
using Dalamud.Memory;
using FFXIVClientStructs.FFXIV.Component.GUI;
using FFXIVClientStructs.Interop;
using FFXIVClientStructs.STD;

namespace CharacterPanelRefined; 

internal static class Util {

    // Traverse a std::map
    internal static unsafe TVal Find<TKey, TVal>(StdMap<Pointer<TKey>, TVal>.Node* node, TKey* item) where TKey : unmanaged where TVal : unmanaged {
        while (!node->IsNil) {
            if (node->KeyValuePair.Item1.Value < item) {
                node = node->Right;
                continue;
            }

            if (node->KeyValuePair.Item1.Value > item) {
                node = node->Left;
                continue;
            }

            return node->KeyValuePair.Item2;
        }

        return default;
    }

    public static unsafe T* CloneNode<T>(T* original) where T : unmanaged {
        var size = sizeof(T);
        var allocation = MemoryHelper.GameAllocateUi((ulong)size);
        var bytes = new byte[size];
        Marshal.Copy(new IntPtr(original), bytes, 0, bytes.Length);
        Marshal.Copy(bytes, 0, allocation, bytes.Length);

        var newNode = (AtkResNode*)allocation;
        newNode->ChildNode = null;
        newNode->ChildCount = 0;
        newNode->PrevSiblingNode = null;
        newNode->NextSiblingNode = null;
        return (T*)newNode;
    }
}
