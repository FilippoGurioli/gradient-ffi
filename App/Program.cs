using System;
using System.Runtime.InteropServices;

int handle = GradientNative.GradientCreate(nodeCount: 10, maxDegree: 3);
GradientNative.GradientSetSource(handle, nodeId: 0, isSource: true);
for (int r = 0; r < 10; r++)
{
    Console.WriteLine($"Round {r}");
    for (int id = 0; id < 10; id++)
    {
        int value = GradientNative.GradientGetValue(handle, id);
        Console.WriteLine($"  Device {id} -> {value}");
    }
    Console.WriteLine();
}

GradientNative.GradientDestroy(handle);

internal static class GradientNative
{
    private const string LibName = "simple_gradient";

    [DllImport(LibName, EntryPoint = "create", CallingConvention = CallingConvention.Cdecl)]
    public static extern int GradientCreate(int nodeCount, int maxDegree);

    [DllImport(LibName, EntryPoint = "destroy", CallingConvention = CallingConvention.Cdecl)]
    public static extern void GradientDestroy(int handle);

    [DllImport(LibName, EntryPoint = "set_source", CallingConvention = CallingConvention.Cdecl)]
    [return: MarshalAs(UnmanagedType.I1)]
    public static extern void GradientSetSource(
        int handle,
        int nodeId,
        [MarshalAs(UnmanagedType.I1)] bool isSource);

    [DllImport(LibName, EntryPoint = "clear_sources", CallingConvention = CallingConvention.Cdecl)]
    public static extern void GradientClearSources(int handle);

    [DllImport(LibName, EntryPoint = "step", CallingConvention = CallingConvention.Cdecl)]
    public static extern void GradientStep(int handle, int rounds);

    [DllImport(LibName, EntryPoint = "get_value", CallingConvention = CallingConvention.Cdecl)]
    public static extern int GradientGetValue(int handle, int nodeId);
}
