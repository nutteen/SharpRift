#region License
//
// VR.cs
//
// Author:
//       Stefanos A. <stapostol@gmail.com>
//
// Copyright (c) 2014 Stefanos Apostolopoulos
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
//
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Runtime.InteropServices;

using SharpRift.Math;

namespace SharpRift
{
    using ovrHmd = HMDisplay;

    public static partial class VR
    {
        const string lib = "libovr";

        [DllImport("kernel32.dll")]
        static extern IntPtr LoadLibrary(string filename);

        static VR()
        {
            // When running on Windows, we need to load the
            // native library into the address space of the
            // application. This allows us to use DllImport
            // to load the correct (x86 or x64) library at
            // runtime.
            // Non-windows platforms rely on the dll.config
            // file to load the correct library.

            if (IntPtr.Size == 4)
            {
                // Running on 32bit system
                LoadLibrary("../../lib/x86/libovr.dll");
            }
            else
            {
                // Running on 64bit system
                LoadLibrary("../../lib/x64/libovr.dll");
            }
        }

        #region OVR Interface
        /// <summary>
        /// Initializes the Oculus Rift API.
        /// This function must be called successfully before
        /// using any Oculus Rift functionality.
        /// </summary>
        /// <returns><c>true</c> if the initialize was successful; <c>false</c> otherwise.</returns>
        [DllImport(lib, EntryPoint = "ovr_Initialize", CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.I1)]
        public static extern bool Initialize();

        /// <summary>
        /// Shuts down the Oculus Rift API.
        /// </summary>
        [DllImport(lib, EntryPoint = "ovr_Shutdown", CallingConvention = CallingConvention.Cdecl)]
        public static extern void Shutdown();
        #endregion

        #region HMD Interface
        /// <summary>
        /// Detects or re-detects HMDs and reports the total number detected.
        /// Users can get information about each HMD by calling <see cref="Create(int)"/> with an index.
        /// </summary>
        /// <returns>The total number of HMDs detected.</returns>
        [DllImport(lib, EntryPoint = "ovrHmd_Detect", CallingConvention = CallingConvention.Winapi)]
        public static extern int Detect();

        /// <summary>
        /// Creates a handle to an HMD.
        /// </summary>
        /// <param name="index">
        /// Index of the HMD, between 0 and <see cref="Detect()"/> - 1.
        /// Note: index mappings can cange after each <see cref="Detect"/>.
        /// </param>
        /// <returns>
        /// A <see cref="System.IntPtr"/> representing a handle to an HMD, or <see cref="System.IntPtr.Zero"/>
        /// if the specified index is not valid.
        /// Valid handles must be freed with <see cref="Destroy"/>.
        /// </returns>
        [DllImport(lib, EntryPoint = "ovrHmd_Create", CallingConvention = CallingConvention.Cdecl)]
        public static extern ovrHmd Create(int index);

        /// <summary>
        /// Destroys the specified HMD handle.
        /// </summary>
        /// <param name="hmd">A valid HMD handle obtained by <see cref="Create"/>.</param>
        [DllImport(lib, EntryPoint = "ovrHmd_Destroy", CallingConvention = CallingConvention.Cdecl)]
        public static extern void Destroy(this ovrHmd hmd);

        /// <summary>
        /// Creates a "fake" HMD used for debugging only. This is not tied to specific hardware,
        /// but may be used to debug some of the related rendering.
        /// </summary>
        /// <returns>A <see cref="System.IntPtr"/> represtenting a debug HMD handle.</returns>
        /// <param name="type">The desired <see cref="HMDisplayType"/>.</param>
        [DllImport(lib, EntryPoint = "ovrHmd_CreateDebug", CallingConvention = CallingConvention.Cdecl)]
        public static extern ovrHmd CreateDebug(HMDisplayType type);

        /// <summary>
        /// Returns a string representing the last HMD error.
        /// To obtain the global error state, use <see cref="GetLastError()"/> instead.
        /// This method allocates managed memory.
        /// </summary>
        /// <returns>A <see cref="System.String"/> representing the last HMD error,
        /// or <see cref="System.String.Empty"/> if no error has occured.
        /// </returns>
        /// <param name="hmd">
        /// An valid HMD handle obtained by <see cref="Create"/> or <see cref="CreateDebug"/>.
        /// </param>
        public static string GetLastError(this ovrHmd hmd)
        {
            return Marshal.PtrToStringAnsi(GetLastErrorInternal(hmd));
        }

        /// <summary>
        /// Returns a string representing the last global error.
        /// To obtain the error state of a specific HMD, use <see cref="GetLastError(HMDisplay)"/> instead.
        /// This method allocates managed memory.
        /// </summary>
        /// <returns>A <see cref="System.String"/> representing the last HMD error,
        /// or <see cref="System.String.Empty"/> if no error has occured.
        /// </returns>
        public static string GetLastError()
        {
            return Marshal.PtrToStringAnsi(GetLastErrorInternal(HMDisplay.Zero));
        }

        [DllImport(lib, EntryPoint = "ovrHmd_GetLastError", CallingConvention = CallingConvention.Winapi)]
        static extern IntPtr GetLastErrorInternal(this ovrHmd hmd);

        /// <summary>
        /// Returns capability bits that are enabled at this time; described by <see cref="HMDisplayCaps"/>.
        /// Note that this value is different from <see cref="HMDisplayDescription.DisplayCaps"/>,
        /// which describes what capabilities are available.
        /// </summary>
        /// <returns>A bitwise combination of <see cref="HMDisplayCaps"/> flags representing the enabled capabilities.</returns>
        /// <param name="hmd">A valid HMD handle.</param>
        [DllImport(lib, EntryPoint = "ovrHmd_GetEnabledCaps", CallingConvention = CallingConvention.Winapi)]
        public static extern HMDisplayCaps GetEnabledCaps(this ovrHmd hmd);

        /// <summary>
        /// Modifies capability bits described by ovrHmdCaps that can be modified.
        /// </summary>
        /// <param name="hmd">A valid HMD handle.</param>
        /// <param name="hmdCaps">A bitwise combination of the <see cref="HMDisplayCaps"/> flags to enable</param>
        [DllImport(lib, EntryPoint = "ovrHmd_SetEnabledCaps", CallingConvention = CallingConvention.Winapi)]
        public static extern void SetEnabledCaps(this ovrHmd hmd, HMDisplayCaps hmdCaps);
        #endregion

        #region Sensor Interface

        // Sensor APIs are separated from Create & Configure for several reasons:
        //  - They need custom parameters that control allocation of heavy resources
        //    such as Vision tracking, which you don't want to create unless necessary.
        //  - A game may want to switch some sensor settings based on user input, 
        //    or at lease enable/disable features such as Vision for debugging.
        //  - The same or syntactically similar sensor interface is likely to be used if we 
        //    introduce controllers.
        //
        //  - Sensor interface functions are all Thread-safe, unlike the frame/render API
        //    functions that have different rules (all frame access functions
        //    must be on render thread)
        [DllImport(lib, EntryPoint = "ovrHmd_ConfigureTracking", CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.I1)]
        public static extern bool ConfigureTracking(this ovrHmd hmd,
            SensorCaps supportedSensorCaps,
            SensorCaps requiredSensorCaps);

        /// <summary>
        /// Starts sensor sampling, enabling specified capabilities, described by <see cref="SensorCaps"/>.
        /// </summary>
        /// <returns><c>true</c>, if sensor was started, <c>false</c> otherwise.</returns>
        /// <param name="hmd">A valid HMD handle.</param>
        /// <param name="supportedSensorCaps">
        /// Sensor capabilities that are requested at the time of the call. The function will succeed 
        /// even if these caps are not available (i.e. sensor or camera is unplugged). Support
        /// will automatically be enabled if such device is plugged in later. Software should
        /// check ovrSensorState.StatusFlags for real-time status.
        /// </param>
        /// <param name="requiredSensorCaps">
        /// Sensor capabilities required at the time of the call.
        /// If they are not available, the function will fail. Pass 0 if only specifying
        /// supportedSensorCaps.
        /// </param>
        /// <remarks>
        /// All sensor interface functions are thread-safe, allowing sensor state to be sampled
        /// from different threads.
        /// </remarks>
//        [DllImport(lib, EntryPoint = "ovrHmd_StartSensor", CallingConvention = CallingConvention.Cdecl)]
//        [return: MarshalAs(UnmanagedType.I1)]
//        public static extern bool StartSensor(this ovrHmd hmd,
//            SensorCaps supportedSensorCaps,
//            SensorCaps requiredSensorCaps);

        /// <summary>
        /// Stops sensor sampling, shutting down internal resources.
        /// </summary>
        /// <param name="hmd">A valid HMD handle.</param>
        /// <remarks>
        /// All sensor interface functions are thread-safe, allowing sensor state to be sampled
        /// from different threads.
        /// </remarks>
        [DllImport(lib, EntryPoint = "ovrHmd_StopSensor", CallingConvention = CallingConvention.Winapi)]
        public static extern void StopSensor(this ovrHmd hmd);

        /// <summary>
        /// Resets sensor orientation.
        /// </summary>
        /// <param name="hmd">A valid HMD handle.</param>
        /// <remarks>
        /// All sensor interface functions are thread-safe, allowing sensor state to be sampled
        /// from different threads.
        /// </remarks>
        [DllImport(lib, EntryPoint = "ovrHmd_ResetSensor", CallingConvention = CallingConvention.Winapi)]
        public static extern void ResetSensor(this ovrHmd hmd);

        /// <summary>
        /// Returns sensor state reading based on the specified absolute system time.
        /// </summary>
        /// <returns>The sensor state.</returns>
        /// <param name="hmd">A valid HMD handle.</param>
        /// <param name="absTime">
        /// Pass absTime value of 0.0 to request the most recent sensor reading; in this case
        /// both PredictedPose and SamplePose will have the same value.
        /// </param>
        /// <remarks>
        /// All sensor interface functions are thread-safe, allowing sensor state to be sampled
        /// from different threads.
        /// </remarks>
        [DllImport(lib, EntryPoint = "ovrHmd_GetSensorState", CallingConvention = CallingConvention.Winapi)]
        public static extern SensorState GetSensorState(this ovrHmd hmd, double absTime);

        /// <summary>
        /// Returns information about a sensor.
        /// Only valid after <see cref="StartSensor"/>.
        /// </summary>
        /// <returns><c>true</c>, if a sensor desc was obtained successfully; <c>false</c> otherwise.</returns>
        /// <param name="hmd">A valid HMD handle.</param>
        /// <param name="descOut">
        /// A non-null <see cref="System.Text.StringBuilder"/> instance.
        /// After a successful call, this will hold the sensor description.
        /// </param>
        /// <remarks>
        /// All sensor interface functions are thread-safe, allowing sensor state to be sampled
        /// from different threads.
        /// </remarks>
        [DllImport(lib, EntryPoint = "ovrHmd_GetSensorDesc", CallingConvention = CallingConvention.Winapi)]
        [return: MarshalAs(UnmanagedType.I1)]
        public static extern bool GetSensorDescription(this ovrHmd hmd, StringBuilder descOut);

        #endregion

        #region Graphics Setup

        /// <summary>
        /// Fills in description about HMD; this is the same as filled in by <see cref="Create"/>.
        /// </summary>
        /// <param name="hmd">A valid HMD handle.</param>
        /// <param name="desc">A <see cref="HMDisplayDescription"/> instance containing information about an HMD.</param>
        [DllImport(lib, EntryPoint = "ovrHmd_GetDesc", CallingConvention = CallingConvention.Winapi)]
        public static extern void GetDescription(this ovrHmd hmd, out HMDisplayDescription desc);

        /// <summary>
        /// Fills in description about HMD; this is the same as filled in by <see cref="Create"/>.
        /// </summary>
        /// <returns>The description.</returns>
        /// <param name="hmd">A valid HMD handle.</param>
        public static HMDisplayDescription GetDescription(this ovrHmd hmd)
        {
            HMDisplayDescription desc;
            GetDescription(hmd, out desc);
            return desc;
        }

        /// <summary>
        /// Gets the recommended texture size for a single eye and given FOV cone.
        /// Higher FOV will generally require larger textures to maintain quality.
        /// </summary>
        /// <returns>The recommended texture size for the specified FOV cone.</returns>
        /// <param name="hmd">A valid HMD handle.</param>
        /// <param name="eye">The <see cref="EyeType"/> for the left or right eye.</param>
        /// <param name="fov">A <see cref="FovPort"/> instance specifying the desired FOV cone.</param>
        /// <param name="pixelsPerDisplayPixel">
        /// Specifies that number of render target pixels per display
        /// pixel at center of distortion; 1.0 is the default value. Lower values
        /// can improve performance.
        /// </param>
        [DllImport(lib, EntryPoint = "ovrHmd_GetFovTextureSize", CallingConvention = CallingConvention.Winapi)]
        public static extern VRSize GetFovTextureSize(this ovrHmd hmd, EyeType eye, FovPort fov,
            float pixelsPerDisplayPixel);

        #endregion

        #region SDK Rendering

        /// <summary>
        /// Configures rendering; fills in computed render parameters.
        /// This function can be called multiple times to change rendering settings.
        /// Pass in two eye view descriptors to generate complete rendering information
        /// for each eye.
        /// </summary>
        /// <returns><c>true</c>, if rendering was configured successfully; <c>false</c> otherwise.</returns>
        /// <param name="hmd">A valid HMD handle.</param>
        /// <param name="apiConfig">
        /// Provides D3D/OpenGL specific parameters. Pass null
        /// to shutdown rendering and release all resources.
        /// </param>
        /// <param name="distortionCaps">
        /// A bitwise combination of <see cref="DistortionCaps"/> flags
        /// that describe the distortion settings that will be applied.
        /// </param>
        /// <param name="eyeFovIn">An array of 2 <see cref="FovPort"/> instances in.</param>
        /// <param name="eyeRenderDescOut">
        /// When the function returns successfully, contains two <see cref="EyeRenderDescription"/>
        /// instances describing the rendering settings for each eye.
        /// </param>
        /// <remarks>
        /// This function supports rendering of distortion by the SDK through direct
        /// access to the underlying rendering HW, such as D3D or GL.
        /// This is the recommended approach, as it allows for better support or future
        /// Oculus hardware and a range of low-level optimizations.
        /// </remarks>
        [DllImport(lib, EntryPoint = "ovrHmd_ConfigureRendering", CallingConvention = CallingConvention.Winapi)]
        [return: MarshalAs(UnmanagedType.I1)]
        public static extern bool ConfigureRendering(this ovrHmd hmd,
            ref RenderConfiguration apiConfig,
            DistortionCaps distortionCaps,
            [MarshalAs(UnmanagedType.LPArray, SizeConst = 2)] FovPort[] eyeFovIn,
            [Out][MarshalAs(UnmanagedType.LPArray, SizeConst = 2)] EyeRenderDescription[] eyeRenderDescOut);

        /// <summary>
        /// Begins a frame, returning timing and orientation information useful for simulation.
        /// This should be called in the beginning of game rendering loop (on render thread).
        /// This function relies on <see cref="BeginFrameTiming"/> for some of its functionality.
        /// </summary>
        /// <returns>A <see cref="FrameTiming"/> instance.</returns>
        /// <param name="hmd">A valid HMD handle.</param>
        /// <param name="frameIndex">The frame index. Pass 0 for frame index if not using GetFrameTiming.</param>
        /// <remarks>
        /// This function supports rendering of distortion by the SDK through direct
        /// access to the underlying rendering HW, such as D3D or GL.
        /// This is the recommended approach, as it allows for better support or future
        /// Oculus hardware and a range of low-level optimizations.
        /// </remarks>
        [DllImport(lib, EntryPoint = "ovrHmd_BeginFrame", CallingConvention = CallingConvention.Winapi)]
        public static extern FrameTiming BeginFrame(this ovrHmd hmd, int frameIndex);

        /// <summary>
        /// Ends frame, rendering textures to frame buffer. This may perform distortion and scaling
        /// internally, assuming is it not delegated to another thread. 
        /// Must be called on the same thread as BeginFrame. Calls <see cref="BeginFrameTiming"/> internally.
        /// Note: this Function will call SwapBuffers() and potentially wait for VSync.
        /// </summary>
        /// <param name="hmd">A valid HMD handle.</param>
        /// <remarks>
        /// This function supports rendering of distortion by the SDK through direct
        /// access to the underlying rendering HW, such as D3D or GL.
        /// This is the recommended approach, as it allows for better support or future
        /// Oculus hardware and a range of low-level optimizations.
        /// </remarks>
        [DllImport(lib, EntryPoint = "ovrHmd_EndFrame", CallingConvention = CallingConvention.Winapi)]
        public static extern void EndFrame(this ovrHmd hmd);

        /// <summary>
        /// Marks beginning of eye rendering. Must be called on the same thread as BeginFrame.
        /// This function uses <see cref="GetEyePose"/> to predict sensor state that should be
        /// used rendering the specified eye.
        /// This combines current absolute time with prediction that is appropriate for this HMD.
        /// It is ok to call BeginEyeRender() on both eyes before calling <see cref="EndEyeRender"/>.
        /// If rendering one eye at a time, it is best to render the eye specified by
        /// <see cref="HMDisplayDescription.EyeRenderOrderFirst"/> first.
        /// </summary>
        /// <returns>An <see cref="EyePose"/> instance.</returns>
        /// <param name="hmd">A valid HMD handle.</param>
        /// <param name="eye">The <see cref="EyeType"/> for the left or right eye.</param>
        /// <remarks>
        /// This function supports rendering of distortion by the SDK through direct
        /// access to the underlying rendering HW, such as D3D or GL.
        /// This is the recommended approach, as it allows for better support or future
        /// Oculus hardware and a range of low-level optimizations.
        /// </remarks>
        [DllImport(lib, EntryPoint = "ovrHmd_BeginEyeRender", CallingConvention = CallingConvention.Winapi)]
        public static extern EyePose BeginEyeRender(this ovrHmd hmd, EyeType eye);

        /// <summary>
        /// Marks the end of eye rendering and submits the eye texture for display after it is ready.
        /// Rendering viewport within the texture can change per frame if necessary.
        /// Specified texture may be presented immediately or wait until <see cref="EndFrame"/> based
        /// on the implementation. The API performs distortion and scaling internally.
        /// </summary>
        /// <param name="hmd">A valid HMD handle.</param>
        /// <param name="eye">The <see cref="EyeType"/> for the left or right eye.</param>
        /// <param name="renderPose">
        /// A <see cref="EyePose"/> returned by <see cref="BeginEyeRender"/>
        /// or a custom <see cref="EyePose"/>.
        /// </param>
        /// <param name="eyeTexture">The <see cref="VRTexture"/> to render.</param>
        /// <remarks>
        /// This function supports rendering of distortion by the SDK through direct
        /// access to the underlying rendering HW, such as D3D or GL.
        /// This is the recommended approach, as it allows for better support or future
        /// Oculus hardware and a range of low-level optimizations.
        /// </remarks>
        [DllImport(lib, EntryPoint = "ovrHmd_EndEyeRender", CallingConvention = CallingConvention.Winapi)]
        public static extern void EndEyeRender(this ovrHmd hmd, EyeType eye,
            EyePose renderPose, ref VRTexture eyeTexture);

        #endregion

        #region Stateless math setup functions

        /// <summary>
        /// Calculates the projection matrix for the specified fov, znear, zfar and rightHanded.
        /// </summary>
        /// <param name="fov">The desired <see cref="FovPort"/>.</param>
        /// <param name="znear">The desired near z-plane.</param>
        /// <param name="zfar">The desired far z-plane.</param>
        /// <param name="rightHanded">
        /// If set to <c>true</c> the returned matrix will be right handed.
        /// </param>
        [DllImport(lib, EntryPoint = "ovrMatrix4f_Projection", CallingConvention = CallingConvention.Winapi)]
        public static extern Matrix4 Projection(FovPort fov,
            float znear, float zfar, [MarshalAs(UnmanagedType.I1)] bool rightHanded);

        /// <summary>
        /// Used for 2D rendering, Y is down
        /// </summary>
        /// <returns>An <see cref="OpenTK.Matrix4"/> represensting an orthographic projection matrix.</returns>
        /// <param name="projection">The parent projection matrix, returned by <see cref="Projection"/>.</param>
        /// <param name="orthoScale">The x/y scale for the orthographic projection (1.0f / pixelsPerTanAngleAtCenter)</param>
        /// <param name="orthoDistance">The distance of the projection plane from the camera (e.g. 0.8m).</param>
        /// <param name="eyeViewAdjustX">X eye adjustment factor.</param>
        [DllImport(lib, EntryPoint = "ovrMatrix4f_OrthoSubProjection", CallingConvention = CallingConvention.Winapi)]
        public static extern Matrix4 OrthoSubProjection(Matrix4 projection, Vector2 orthoScale,
            float orthoDistance, float eyeViewAdjustX);

        /// <summary>
        /// Returns global, absolute high-resolution time in seconds. This is the same
        /// value as used in sensor messages.
        /// </summary>
        /// <returns>The time in seconds.</returns>
        [DllImport(lib, EntryPoint = "ovr_GetTimeInSeconds", CallingConvention = CallingConvention.Winapi)]
        public static extern double GetTimeInSeconds();

        /// <summary>
        /// Waits until the specified absolute time.
        /// </summary>
        /// <returns>The absolute time that was waited.</returns>
        /// <param name="absTime">The absolute time to wait.</param>
        [DllImport(lib, EntryPoint = "ovr_WaitTillTime", CallingConvention = CallingConvention.Winapi)]
        public static extern double WaitTillTime(double absTime);

        #endregion

        #region Latency Test interface

        /// <summary>
        /// Performs latency test processing and returns 'TRUE' if specified rgb color should
        /// be used to clear the screen.
        /// </summary>
        /// <returns><c>true</c>, if the latency test conluded successfully, <c>false</c> otherwise.</returns>
        /// <param name="hmd">A valid HMD handle.</param>
        /// <param name="rgbColorOut">
        /// When this function returns successfully, contains the optimal RGB color
        /// for clearing the screen.
        /// </param>
        [DllImport(lib, EntryPoint = "ovrHmd_ProcessLatencyTest", CallingConvention = CallingConvention.Winapi)]
        [return: MarshalAs(UnmanagedType.I1)]
        public static extern bool ProcessLatencyTest(this ovrHmd hmd,
            [Out][MarshalAs(UnmanagedType.LPStr, SizeConst = 3)] byte rgbColorOut);

        /// <summary>
        /// Returns the results of the latency test. This is a blocking call.
        /// </summary>
        /// <returns>A pointer to a null-terminated string that contains the results of the latency test.</returns>
        /// <param name="hmd">A valid HMD handle.</param>
        [DllImport(lib, EntryPoint = "ovrHmd_GetLatencyTestResult", CallingConvention = CallingConvention.Winapi)]
        public static extern IntPtr GetLatencyTestResultPointer(this ovrHmd hmd);

        /// <summary>
        /// Returns the results of the latency test. This is a blocking call.
        /// Note: this function allocates memory.
        /// </summary>
        /// <returns>A <see cref="System.String"/> that contains the results of the latency test.</returns>
        /// <param name="hmd">A valid HMD handle.</param>
        public static string GetLatencyTestResult(this ovrHmd hmd)
        {
            return Marshal.PtrToStringAnsi(GetLatencyTestResultPointer(hmd));
        }

        /// <summary>
        /// Returns latency for HMDs that support internal latency testing via the
        /// pixel-read back method.
        /// </summary>
        /// <returns>
        /// The results of the latency test in seconds, or -1 if the latency test failed.
        /// </returns>
        /// <param name="hmd">A valid HMD handle.</param>
        [DllImport(lib, EntryPoint = "ovrHmd_GetMeasuredLatencyTest2", CallingConvention = CallingConvention.Winapi)]
        public static extern double GetMeasuredLatencyTest2(this ovrHmd hmd);

        #endregion
    }

    #region Enumerations

    /// <summary>
    /// Enumerates all head-mounted display types that we support.
    /// </summary>
    public enum HMDisplayType
    {
        /// <summary>
        /// Head-mounted displays are not supported
        /// </summary>
        None = 0,

        /// <summary>
        /// The Development Kit v1
        /// </summary>
        DK1 = 3,

        /// <summary>
        /// The High-Definition Development Kit
        /// </summary>
        DKHD = 4,

        /// <summary>
        /// The Development Kit v2
        /// </summary>
        DK2 = 6,

        /// <summary>
        /// Some HMD other then the one in the enumeration
        /// </summary>
        Other
    }

    /// <summary>
    /// Head-mounted display capability flags reported by device.
    /// </summary>
    [Flags]
    public enum HMDisplayCaps
    {
        /// <summary>
        /// This HMD exists (as opposed to being unplugged).
        /// This is a read-only flag.
        /// </summary>
        Present = 0x0001,

        /// <summary>
        /// HMD and sensor are available for use
        /// (if not owned by another app).
        /// This is a read-only flag.
        /// </summary>
        Available = 0x0002,

        // These flags are intended for use with the new driver display mode.
        /*
            ovrHmdCap_ExtendDesktop     = 0x0004,   // Read only, means display driver is in compatibility mode.
            ovrHmdCap_DisplayOff        = 0x0040,   // Turns off Oculus HMD screen and output.
            ovrHmdCap_NoMirrorToWindow  = 0x2000,   // Disables mirrowing of HMD output to the window;
                                                    // may improve rendering performance slightly.
            */

        /// <summary>
        /// Supports low persistence mode.
        /// This flag can be modified by <see cref="OpenTK.VR.SetEnabledCaps"/>
        /// </summary>
        LowPersistence = 0x0080,

        /// <summary>
        /// Supports pixel reading for continuous latency testing.
        /// This flag can be modified by <see cref="OpenTK.VR.SetEnabledCaps"/>
        /// </summary>
        LatencyTest = 0x0100,

        /// <summary>
        /// Adjust prediction dynamically based on DK2 Latency.
        /// This flag can be modified by <see cref="OpenTK.VR.SetEnabledCaps"/>
        /// </summary>
        DynamicPrediction = 0x0200,

        /// <summary>
        /// Support rendering without VSync for debugging
        /// This flag can be modified by <see cref="OpenTK.VR.SetEnabledCaps"/>
        /// </summary>
        NoVSync = 0x1000,

        /// <summary>
        /// 
        /// This flag can be modified by <see cref="OpenTK.VR.SetEnabledCaps"/>
        /// </summary>
        NoRestore = 0x4000,

        /// <summary>
        /// Defines which bits can be modified by <see cref="OpenTK.VR.SetEnabledCaps"/>
        /// </summary>
        WritableMask = 0x1380
    }

    /// <summary>
    /// Sensor capability bits reported by device.
    /// Used with <see cref="VR.StartSensor"/>.
    /// </summary>
    [Flags]
    public enum SensorCaps
    {
        /// <summary>
        /// Supports orientation tracking (IMU).
        /// </summary>
        Orientation = 0x0010,

        /// <summary>
        /// Supports yaw correction through magnetometer or other means.
        /// </summary>
        YawCorrection = 0x0020,

        /// <summary>
        /// Supports positional tracking.
        /// </summary>
        Position = 0x0040,
    }

    /// <summary>
    /// Distortion capability bits reported by device.
    /// Used with <see cref="OpenTK.VR.ConfigureRendering"/> and
    /// <see cref="OpenTK.VR.CreateDistortionMesh"/>.
    /// </summary>
    [Flags]
    public enum DistortionCaps
    {
        /// <summary>
        /// Supports chromatic aberration correction.
        /// </summary>
        Chromatic = 0x01,

        /// <summary>
        /// Supports timewarp.
        /// </summary>
        TimeWarp = 0x02,

        /// <summary>
        /// Supports vignetting around the edges of the view.
        /// </summary>
        Vignette = 0x08
    }

    /// <summary>
    /// Specifies which eye is being used for rendering.
    /// This type explicitly does not include a third "NoStereo" option, as such is
    /// not required for an HMD-centered API.
    /// </summary>
    public enum EyeType
    {
        /// <summary>
        /// The left eye.
        /// </summary>
        Left = 0,

        /// <summary>
        /// The right eye.
        /// </summary>
        Right = 1,

        /// <summary>
        /// The number of eyes (2).
        /// </summary>
        Count = 2
    }

    /// <summary>
    /// Bit flags describing the current status of sensor tracking.
    /// </summary>
    [Flags]
    public enum StatusFlags
    {
        /// <summary>
        /// Orientation is currently tracked (connected and in use).
        /// </summary>
        OrientationTracked = 0x0001,

        /// <summary>
        /// Position is currently tracked (false if out of range).
        /// </summary>
        PositionTracked = 0x0002,

        /// <summary>
        /// Position tracking HW is connected.
        /// </summary>
        PositionConnected = 0x0020,

        /// <summary>
        /// HMD Display is connected and available.
        /// </summary>
        HMDisplayConnected = 0x0080
    }

    /// <summary>
    /// These types are used to hide platform-specific details when passing
    /// render device, OS and texture data to the APIs.
    /// </summary>
    /// <remarks>
    /// The benefit of having these wrappers vs. platform-specific API functions is
    /// that they allow game glue code to be portable. A typical example is an
    /// engine that has multiple back ends, say GL and D3D. Portable code that calls
    /// these back ends may also use LibOVR. To do this, back ends can be modified
    /// to return portable types such as ovrTexture and ovrRenderAPIConfig.
    /// </remarks>
    public enum RenderApiType
    {
        /// <summary>
        /// No rendering API.
        /// </summary>
        None,

        /// <summary>
        /// The desktop OpenGL API.
        /// </summary>
        OpenGL,

        /// <summary>
        /// The Android OpenGL ES API.
        /// May include extra native window pointers, etc.
        /// </summary>
        Android_GLES,

        /// <summary>
        /// The Windows Direct3D 9 API.
        /// </summary>
        D3D9,

        /// <summary>
        /// The Windows Direct3D 10 API.
        /// </summary>
        D3D10,

        /// <summary>
        /// The Windows Direct3D 11 API.
        /// </summary>
        D3D11,

        /// <summary>
        /// The maximum number of supported APIs.
        /// </summary>
        Count
    }

    #endregion

    #region Structures

    /// <summary>
    /// Defines a 2d point.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct VRPoint
    {
        /// <summary>
        /// The x coordinate of this <see cref="VRPoint"/>.
        /// </summary>
        public int X;

        /// <summary>
        /// The y coordinate of this <see cref="VRPoint"/>.
        /// </summary>
        public int Y;

        /// <summary>
        /// Initializes a new instance of the <see cref="OpenTK.VRPoint"/> struct.
        /// </summary>
        /// <param name="x">The x coordinate.</param>
        /// <param name="y">The y coordinate.</param>
        public VRPoint(int x, int y)
        {
            X = x;
            Y = y;
        }
    }

    /// <summary>
    /// Defines a 2d size.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct VRSize
    {
        /// <summary>
        /// The width of this <see cref="VRSize"/>.
        /// </summary>
        public int Width;

        /// <summary>
        /// The height of this <see cref="VRSize"/>.
        /// </summary>
        public int Height;

        /// <summary>
        /// Initializes a new instance of the <see cref="OpenTK.VRSize"/> struct.
        /// </summary>
        /// <param name="width">The width of this instance.</param>
        /// <param name="height">The height of this instance.</param>
        public VRSize(int width, int height)
        {
            Width = width;
            Height = height;
        }
    }

    /// <summary>
    /// Defines a 2d rectangle.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct VRRectangle
    {
        /// <summary>
        /// An <see cref="VRPoint"/> resresenting the top-left corner of this <see cref="VRRectangle"/>.
        /// </summary>
        public VRPoint Location;

        /// <summary>
        /// An <see cref="VRSize"/> resresenting the width and height of this <see cref="VRRectangle"/>.
        /// </summary>
        public VRSize Size;

        /// <summary>
        /// Gets or sets the x coordinate of this <see cref="VRRectangle"/>.
        /// </summary>
        public int X
        {
            get { return Location.X; }
            set { Location.X = value; }
        }

        /// <summary>
        /// Gets or sets the y coordinate of this <see cref="VRRectangle"/>.
        /// </summary>
        public int Y
        {
            get { return Location.Y; }
            set { Location.Y = value; }
        }

        /// <summary>
        /// Gets or sets the width of this <see cref="VRRectangle"/>.
        /// </summary>
        public int Width
        {
            get { return Size.Width; }
            set { Size.Width = value; }
        }

        /// <summary>
        /// Gets or sets the height of this <see cref="VRRectangle"/>.
        /// </summary>
        public int Height
        {
            get { return Size.Height; }
            set { Size.Height = value; }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="OpenTK.VRRectangle"/> struct.
        /// </summary>
        /// <param name="x">The x coordinate.</param>
        /// <param name="y">The y coordinate.</param>
        /// <param name="width">The width of this rectangle.</param>
        /// <param name="height">The height of this rectangle.</param>
        public VRRectangle(int x, int y, int width, int height)
        {
            Location = new VRPoint(x, y);
            Size = new VRSize(width, height);
        }
    }

    /// <summary>
    /// Position and orientation together.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct EyePose
    {
        /// <summary>
        /// A <see cref="OpenTK.Quaternion"/> describing the orientation of this <see cref="EyePose"/>.
        /// </summary>
        public Quaternion Orientation;

        /// <summary>
        /// A <see cref="OpenTK.Vector3"/> describing the position of this <see cref="EyePose"/>.
        /// </summary>
        public Vector3 Position;
    }

    /// <summary>
    /// Full pose (rigid body) configuration with first and second derivatives.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct PoseState
    {
        /// <summary>
        /// The <see cref="EyePose"/> for this <see cref="PoseState"/>.
        /// </summary>
        public EyePose Pose;

        /// <summary>
        /// A <see cref="OpenTK.Vector3"/> representing the angular velocity of this <see cref="PoseState"/>.
        /// </summary>
        public Vector3 AngularVelocity;

        /// <summary>
        /// A <see cref="OpenTK.Vector3"/> representing the linear velocity of this <see cref="PoseState"/>.
        /// </summary>
        public Vector3 LinearVelocity;

        /// <summary>
        /// A <see cref="OpenTK.Vector3"/> representing the angular acceleration of this <see cref="PoseState"/>.
        /// </summary>
        public Vector3 AngularAcceleration;

        /// <summary>
        /// A <see cref="OpenTK.Vector3"/> representing the linear acceleration of this <see cref="PoseState"/>.
        /// </summary>
        public Vector3 LinearAcceleration;

        /// <summary>
        /// Absolute time of this state sample.
        /// </summary>
        public double TimeInSeconds;
    }

    /// <summary>
    /// Field Of View (FOV) in tangent of the angle units.
    /// As an example, for a standard 90 degree vertical FOV, we would 
    /// have: { UpTan = tan(90 degrees / 2), DownTan = tan(90 degrees / 2) }.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct FovPort
    {
        /// <summary>
        /// The up tangent for this <see cref="FovPort"/>.
        /// </summary>
        public float UpTan;

        /// <summary>
        /// The down tangent for this <see cref="FovPort"/>.
        /// </summary>
        public float DownTan;

        /// <summary>
        /// The left tangent for this <see cref="FovPort"/>.
        /// </summary>
        public float LeftTan;

        /// <summary>
        /// The right tangent for this <see cref="FovPort"/>.
        /// </summary>
        public float RightTan;
    }

    /// <summary>
    /// Describes a head-mounted display device.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct HMDisplayDescription
    {
        /// <summary>
        /// Handle of this HMD.
        /// </summary>
        public ovrHmd Handle;

        /// <summary>
        /// A <see cref="HMDisplayType"/> value representing the type of the display.
        /// </summary>
        public HMDisplayType Type;

        IntPtr ProductNamePtr;
        IntPtr ManufacturerPtr;

        /// <summary>
        /// A <see cref="System.String"/> describing the product: "Oculus Rift DK1", etc.
        /// </summary>
        public string ProductName
        {
            get { return Marshal.PtrToStringAnsi(ProductNamePtr); }
        }

        /// <summary>
        /// A <see cref="System.String"/> describing the manufacturer of the product.
        /// </summary>
        /// <value>The manufacturer.</value>
        public string Manufacturer
        {
            get { return Marshal.PtrToStringAnsi(ManufacturerPtr); }
        }

        /// <summary>
        /// Capability bits described by <see cref="HMDisplayCaps"/>.
        /// </summary>
        public HMDisplayCaps DisplayCaps;

        /// <summary>
        /// Capability bits described by <see cref="SensorCaps"/>.
        /// </summary>
        public SensorCaps SensorCaps;

        /// <summary>
        /// Capability bits described by <see cref="DistortionCaps"/> .
        /// </summary>
        public DistortionCaps DistortionCaps;

        /// <summary>
        /// Resolution of the entire HMD screen (for both eyes) in pixels.
        /// </summary>
        public VRSize Resolution;

        /// <summary>
        /// Where monitor window should be on screen or (0,0).
        /// </summary>
        public VRPoint WindowPos;

        /// <summary>
        /// Recommended (default) optical FOV for the left eye.
        /// </summary>
        public FovPort DefaultEyeFovLeft;

        /// <summary>
        /// Recommended (default) optical FOV for the right eye.
        /// </summary>
        public FovPort DefaultEyeFovRight;

        /// <summary>
        /// Maximum optical FOVs for the left eye.
        /// </summary>
        public FovPort MaxEyeFovLeft;

        /// <summary>
        /// Maximum optical FOVs for the right eye.
        /// </summary>
        public FovPort MaxEyeFovRight;

        /// <summary>
        /// Preferred eye rendering order for best performance.
        /// Can help reduce latency on sideways-scanned screens.
        /// </summary>
        public EyeType EyeRenderOrderFirst;

        /// <summary>
        /// Preferred eye rendering order for best performance.
        /// Can help reduce latency on sideways-scanned screens.
        /// </summary>
        public EyeType EyeRenderOrderSecond;

        // Display that HMD should present on.
        // TBD: It may be good to remove this information relying on WidowPos instead.
        // Ultimately, we may need to come up with a more convenient alternative,
        // such as a API-specific functions that return adapter, ot something that will
        // work with our monitor driver.

        IntPtr DisplayDeviceNamePtr;

        /// <summary>
        /// Returns the name of the display: "\\\\.\\DISPLAY3", etc. Can be used in EnumDisplaySettings/CreateDC.
        /// This property is only valid on Windows; on other platforms it will return <see cref="System.String.Empty"/>
        /// </summary>
        public string DisplayDeviceName
        {
            get
            {
                if (DisplayDeviceNamePtr != IntPtr.Zero)
                {
                    return Marshal.PtrToStringAnsi(DisplayDeviceNamePtr);
                }
                return String.Empty;
            }
        }

        /// <summary>
        /// Returns the id of the display.
        /// This property is only valid on Mac OS X.
        /// </summary>
        public int DisplayId;
    }

    /// <summary>
    /// State of the sensor at a given absolute time.
    /// </summary>
    public struct SensorState
    {
        /// <summary>
        /// Predicted pose configuration at requested absolute time.
        /// One can determine the time difference between predicted and actual
        /// readings by comparing <see cref="PoseState.TimeInSeconds"/>.
        /// </summary>
        public PoseState Predicted;

        /// <summary>
        /// Actual recorded pose configuration based on the sensor sample at a 
        /// moment closest to the requested time.
        /// </summary>
        public PoseState Recorded;

        /// <summary>
        /// Sensor temperature reading, in degrees Celsius, as sample time.
        /// </summary>
        public float Temperature;

        /// <summary>
        /// Sensor status described by <see cref="StatusFlags"/>.
        /// </summary>
        public StatusFlags StatusFlags;
    }

    /// <summary>
    /// Rendering information for each eye, computed either by <see cref="OpenTK.VR.ConfigureRendering"/>
    /// or by <see cref="OpenTK.VR.GetRenderDescription"/>, based on the specified Fov.
    /// Note that the rendering viewport is not included here as it can be 
    /// specified separately and modified per frame though:
    ///    (a) calling <see cref="OpenTK.VR.GetRenderScaleAndOffset"/>with game-rendered api,
    /// or (b) passing different values in <see cref="VRTexture"/> in case of SDK-rendered distortion.
    /// </summary>
    public struct EyeRenderDescription
    {
        /// <summary>
        /// The <see cref="EyeType"/> described by this instance.
        /// </summary>
        public EyeType Eye;

        /// <summary>
        /// The <see cref="FovPort"/> configuration for this instance.
        /// </summary>
        public FovPort Fov;

        /// <summary>
        /// An <see cref="VRRectangle"/> representing the viewport after distortion.
        /// </summary>
        public VRRectangle DistortedViewport;

        /// <summary>
        /// A <see cref="OpenTK.Vector2"/> represting the number of display pixels that will fit in tan(angle) = 1.
        /// </summary>
        public Vector2 PixelsPerTanAngleAtCenter;

        /// <summary>
        /// A <see cref="OpenTK.Vector3"/> represting the translation that must be applied to the view matrix.
        /// </summary>
        public Vector3 ViewAdjust;
    }

    /// <summary>
    /// Describes a vertex used for distortion; this is intended to be converted into
    /// the engine-specific format.
    /// Some fields may be unused based on the selected <see cref="DistortionCaps"/>.
    /// TexG and TexB, for example, are not used if chromatic correction is not requested.
    /// </summary>
    public struct DistortionVertex
    {
        /// <summary>
        /// The vertex position.
        /// </summary>
        public Vector2 Position;

        /// <summary>
        /// Lerp factor between time-warp matrices. Can be encoded in Position.Z
        /// </summary>
        public float TimeWarpFactor;

        /// <summary>
        /// Vignette fade factor. Can be encoded in Position.W
        /// </summary>
        public float VignetteFactor;

        /// <summary>
        /// Texture R coordinates.
        /// </summary>
        public Vector2 TexR;

        /// <summary>
        /// Texture G coordinates.
        /// </summary>
        public Vector2 TexG;

        /// <summary>
        /// Texture B coordinates.
        /// </summary>
        public Vector2 TexB;
    }

    /// <summary>
    /// Describes a full set of distortion mesh data, filled in by <see cref="OpenTK.VR.CreateDistortionMesh"/>.
    /// Contents of this data structure, if not null, should be freed by <see cref="OpenTK.VR.DestroyDistortionMesh"/>.
    /// </summary>
    public struct DistortionMesh
    {
        /// <summary>
        /// An unmanaged array of <see cref="DistortionVertex"/> structures.
        /// </summary>
        public IntPtr VertexData;

        /// <summary>
        /// An unmanaged array of <see cref="System.UInt16"/> indices.
        /// </summary>
        public IntPtr IndexData;

        /// <summary>
        /// The number of <see cref="DistortionVertex"/> elements in <see cref="DistortionMesh.VertexData"/>.
        /// </summary>
        public int VertexCount;

        /// <summary>
        /// The number of <see cref="System.UInt16"/> elements in <see cref="DistortionMesh.IndexData"/>.
        /// </summary>
        public int IndexCount;
    }

    /// <summary>
    /// Represents frame timing information reported by <see cref="OpenTK.VR.BeginFrameTiming"/>
    /// <para>
    /// It is generally expected that the following hold:
    /// ThisFrameSeconds &lt; TimewarpPointSeconds &lt; NextFrameSeconds &lt; 
    /// EyeScanoutSeconds[EyeOrderFirst] &lt;= ScanoutMidpointSeconds &lt;= EyeScanoutSeconds[EyeOrderSecond]
    /// </para>
    /// </summary>
    public struct FrameTiming
    {
        /// <summary>
        /// The amount of time that has passed since the previous frame returned
        /// BeginFrameSeconds value, usable for movement scaling.
        /// This will be clamped to no more than 0.1 seconds to prevent
        /// excessive movement after pauses for loading or initialization.
        /// </summary>
        public float DeltaSeconds;

        /// <summary>
        /// Absolute time value of when rendering of this frame began or is expected to
        /// begin; generally equal to NextFrameSeconds of the previous frame. Can be used
        /// for animation timing.
        /// </summary>
        public double ThisFrameSeconds;

        /// <summary>
        /// Absolute point when IMU expects to be sampled for this frame.
        /// </summary>
        public double TimewarpPointSeconds;

        /// <summary>
        /// Absolute time when frame Present + GPU Flush will finish, and the next frame starts.
        /// </summary>
        public double NextFrameSeconds;

        /// <summary>
        /// Time when when half of the screen will be scanned out. Can be passes as a prediction
        /// value to <see cref="OpenTK.VR.GetSensorState"/> to get general orientation.
        /// </summary>
        public double ScanoutMidpointSeconds;

        /// <summary>
        /// Timing points when each eye will be scanned out to display. Used for rendering each eye. 
        /// </summary>
        public double EyeScanoutSecondsLeft;

        /// <summary>
        /// Timing points when each eye will be scanned out to display. Used for rendering each eye. 
        /// </summary>
        public double EyeScanoutSecondsRight;
    }

    /// <summary>
    /// A platform-independent rendering configuration, which is
    /// used in <see cref="OpenTK.VR.ConfigureRendering"/>.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct RenderConfiguration
    {
        /// <summary>
        /// The <see cref="RenderApiType"/> for this configuration.
        /// </summary>
        public RenderApiType Api;

        /// <summary>
        /// An <see cref="VRSize"/> instance representing the size of the render texture.
        /// </summary>
        public VRSize RTSize;

        /// <summary>
        /// An <see cref="System.Int32"/> representing the antialiasing sample count.
        /// </summary>
        public int Multisample;

        // Platform-specific configuration
#pragma warning disable 0169
        IntPtr PlatformData0;
        IntPtr PlatformData1;
        IntPtr PlatformData2;
        IntPtr PlatformData3;
        IntPtr PlatformData4;
        IntPtr PlatformData5;
        IntPtr PlatformData6;
        IntPtr PlatformData7;
#pragma warning restore 0169
    }

    /// <summary>
    /// A platform-independent texture description for
    /// <see cref="OpenTK.VR.EndFrame" />.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct VRTextureHeader
    {
        /// <summary>
        /// The <see cref="RenderApiType"/> for this configuration.
        /// </summary>
        public RenderApiType Api;

        /// <summary>
        /// An <see cref="VRSize"/> instance representing the size of this texture.
        /// </summary>
        public VRSize TextureSize;

        /// <summary>
        /// Pixel viewport in texture that holds eye image.
        /// </summary>
        public VRRectangle RenderViewport;
    }

    /// <summary>
    /// Defines the platform-specific portion of a <see cref="VRTexture"/>
    /// </summary>
    [StructLayout(LayoutKind.Explicit)]
    public struct VRTexture
    {
        /// <summary>
        /// Platform-independent information
        /// </summary>
        [FieldOffset(0)]
        public VRTextureHeader Header;

        /// <summary>
        /// Defines OpenGL-specific data for <see cref="VRTexture"/>
        /// </summary>
        [FieldOffset(0)]
        public VRTextureGL GL;

        /// <summary>
        /// Defines D3D11-specific data for <see cref="VRTexture"/>
        /// </summary>
        [FieldOffset(0)]
        public VRTextureD3D11 D3D11;

        /// <summary>
        /// Defines D3D10-specific data for <see cref="VRTexture"/>
        /// </summary>
        [FieldOffset(0)]
        public VRTextureD3D10 D3D10;

        /// <summary>
        /// Defines D3D9-specific data for <see cref="VRTexture"/>
        /// </summary>
        [FieldOffset(0)]
        public VRTextureD3D9 D3D9;

#pragma warning disable 0169
        [FieldOffset(0)]
        VRTextureOpaque Opaque;
#pragma warning restore 0169
    }

    /// <summary>
    /// Defines D3D11-specific data for <see cref="VRTexture"/>
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct VRTextureD3D11
    {
        /// <summary>
        /// Platform-independent information
        /// </summary>
        public VRTextureHeader Header;

        /// <summary>
        /// An ID3D11Texture2D pointer
        /// </summary>
        public IntPtr Texture;

        /// <summary>
        /// An ID3D11ShaderResourceView pointer
        /// </summary>
        public IntPtr SRView;
    }

    /// <summary>
    /// Defines D3D10-specific data for <see cref="VRTexture"/>
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct VRTextureD3D10
    {
        /// <summary>
        /// Platform-independent information
        /// </summary>
        public VRTextureHeader Header;

        /// <summary>
        /// An ID3D10Texture2D pointer
        /// </summary>
        public IntPtr Texture;

        /// <summary>
        /// An ID3D10ShaderResourceView pointer
        /// </summary>
        public IntPtr SRView;
    }

    /// <summary>
    /// Defines D3D9-specific data for <see cref="VRTexture"/>
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct VRTextureD3D9
    {
        /// <summary>
        /// Platform-independent information
        /// </summary>
        public VRTextureHeader Header;

        /// <summary>
        /// An IDirect3DTexture9 pointer
        /// </summary>
        public IntPtr Texture;
    }

    /// <summary>
    /// Defines OpenGL-specific data for <see cref="VRTexture"/>
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct VRTextureGL
    {
        /// <summary>
        /// Platform-independent information
        /// </summary>
        public VRTextureHeader Header;

        /// <summary>
        /// An OpenGL texture handle generated via <see cref="OpenTK.Graphics.OpenGL.GL.GenTexture()"/>
        /// </summary>
        public int TexId;
    }

    [StructLayout(LayoutKind.Sequential)]
    struct VRTextureOpaque
    {
        /// <summary>
        /// Platform-independent information
        /// </summary>
        public VRTextureHeader Header;

        // Platform-specific configuration
#pragma warning disable 0169
        IntPtr PlatformData0;
        IntPtr PlatformData1;
        IntPtr PlatformData2;
        IntPtr PlatformData3;
        IntPtr PlatformData4;
        IntPtr PlatformData5;
        IntPtr PlatformData6;
        IntPtr PlatformData7;
#pragma warning restore 0169
    }

    /// <summary>
    /// Represents a head-mounted display.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public partial struct HMDisplay : IComparable<HMDisplay>, IEquatable<HMDisplay>, IDisposable
    {
        /// <summary>
        /// Gets the handle for this instance.
        /// </summary>
        /// <value>The handle.</value>
        public IntPtr Handle { get; private set; }

        /// <summary>
        /// Defines a Zero (invalid) instance.
        /// </summary>
        public static readonly HMDisplay Zero =
            new HMDisplay();

        #region Public members

        /// <summary>
        /// Determines whether the specified <see cref="System.Object"/> is equal to the current <see cref="OpenTK.HMDisplay"/>.
        /// </summary>
        /// <param name="obj">The <see cref="System.Object"/> to compare with the current <see cref="OpenTK.HMDisplay"/>.</param>
        /// <returns><c>true</c> if the specified <see cref="System.Object"/> is equal to the current
        /// <see cref="OpenTK.HMDisplay"/>; otherwise, <c>false</c>.</returns>
        public override bool Equals(object obj)
        {
            return
                (obj is HMDisplay) &&
                ((HMDisplay)obj).Equals(this);
        }

        /// <summary>
        /// Serves as a hash function for a <see cref="OpenTK.HMDisplay"/> object.
        /// </summary>
        /// <returns>A hash code for this instance that is suitable for use in hashing algorithms and data structures such as a
        /// hash table.</returns>
        public override int GetHashCode()
        {
            return Handle.GetHashCode();
        }

        /// <summary>
        /// Returns a <see cref="System.String"/> that represents the current <see cref="OpenTK.HMDisplay"/>.
        /// </summary>
        /// <returns>A <see cref="System.String"/> that represents the current <see cref="OpenTK.HMDisplay"/>.</returns>
        public override string ToString()
        {
            return string.Format("[HMDisplay: Handle={0}]", Handle);
        }

        /// <summary>Tests two <see cref="HMDisplay"/> instances for equality.</summary>
        /// <param name="left">The <see cref="HMDisplay"/> instance at the left-hand side of the operator.</param>
        /// <param name="right">The <see cref="HMDisplay"/> instance at the right-hand side of the operator.</param>
        public static bool operator ==(HMDisplay left, HMDisplay right)
        {
            return left.Equals(right);
        }

        /// <summary>Tests two <see cref="HMDisplay"/> instances for inequality.</summary>
        /// <param name="left">The <see cref="HMDisplay"/> instance at the left-hand side of the operator.</param>
        /// <param name="right">The <see cref="HMDisplay"/> instance at the right-hand side of the operator.</param>
        public static bool operator !=(HMDisplay left, HMDisplay right)
        {
            return !left.Equals(right);
        }

        #endregion

        #region IComparable implementation

        /// <summary>
        /// Returns the sort order of the current instance compared to the specified <see cref="HMDisplay"/>.
        /// </summary>
        /// <returns>
        /// A negative number if the current instance is less than other;
        /// a positive number if more; zero if equal.
        /// </returns>
        /// <param name="other">Other.</param>
        public int CompareTo(HMDisplay other)
        {
            return ((long)Handle).CompareTo((long)other.Handle);
        }

        #endregion

        #region IEquatable implementation

        /// <summary>
        /// Determines whether the specified <see cref="OpenTK.HMDisplay"/> is equal to the current <see cref="OpenTK.HMDisplay"/>.
        /// </summary>
        /// <param name="other">The <see cref="OpenTK.HMDisplay"/> to compare with the current <see cref="OpenTK.HMDisplay"/>.</param>
        /// <returns><c>true</c> if the specified <see cref="OpenTK.HMDisplay"/> is equal to the current
        /// <see cref="OpenTK.HMDisplay"/>; otherwise, <c>false</c>.</returns>
        public bool Equals(HMDisplay other)
        {
            return Handle == other.Handle;
        }

        #endregion

        #region IDisposable implementation

        /// <summary>
        /// Releases all resource used by the <see cref="OpenTK.HMDisplay"/> object.
        /// </summary>
        /// <remarks>Call <see cref="Dispose"/> when you are finished using the <see cref="OpenTK.HMDisplay"/>. The
        /// <see cref="Dispose"/> method leaves the <see cref="OpenTK.HMDisplay"/> in an unusable state. After calling
        /// <see cref="Dispose"/>, you must release all references to the <see cref="OpenTK.HMDisplay"/> so the garbage
        /// collector can reclaim the memory that the <see cref="OpenTK.HMDisplay"/> was occupying.</remarks>
        public void Dispose()
        {
            if (Handle != IntPtr.Zero)
            {
                VR.Destroy(this);
                Handle = IntPtr.Zero;
            }
        }

        #endregion
    }


    #endregion
}
