using System;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using Tao.Sdl;

namespace cstari.chips
{
    /// <summary>
    /// Audio channel class.  Currently does absolutely nothing.
    /// </summary>

    public static class AudioMixer
    {
        private static byte[] poly0 = new byte[] { 
            1 };
        private static byte[] poly1 = new byte[] { 
            1, 0 };
        private static byte[] poly2 = new byte[] { 
            1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
        private static byte[] poly4 = new byte[] { 
            1, 0, 0, 1, 1, 0, 1, 0, 1, 1, 1, 1, 0, 0, 0, 0 };
        private static byte[] poly5 = new byte[] { 
            1, 0, 0, 1, 0, 1, 1, 0, 0, 1, 1, 1, 1, 1, 0, 0, 
            0, 0, 1, 1, 0, 1, 1, 1, 0, 1, 0, 1, 0, 0, 0, 0, 
            0 };
        private static byte[] poly9 = new byte[] { 
            1, 0, 0, 0, 0, 1, 0, 0, 0, 1, 1, 0, 0, 0, 0, 1, 
            0, 0, 1, 1, 1, 0, 0, 1, 0, 1, 0, 1, 0, 1, 1, 0, 
            0, 0, 0, 1, 1, 0, 1, 1, 1, 1, 0, 1, 0, 0, 1, 1, 
            0, 1, 1, 1, 0, 0, 1, 0, 0, 0, 1, 0, 1, 0, 0, 0, 
            0, 1, 0, 1, 0, 1, 1, 0, 1, 0, 0, 1, 1, 1, 1, 1, 
            1, 0, 1, 1, 0, 0, 1, 0, 0, 1, 0, 0, 1, 0, 1, 1, 
            0, 1, 1, 1, 1, 1, 1, 0, 0, 1, 0, 0, 1, 1, 0, 1, 
            0, 1, 0, 0, 1, 1, 0, 0, 1, 1, 0, 0, 0, 0, 0, 0, 
            0, 1, 1, 0, 0, 0, 1, 1, 0, 0, 1, 0, 1, 0, 0, 0, 
            1, 1, 0, 1, 0, 0, 1, 0, 1, 1, 1, 1, 1, 1, 1, 0, 
            1, 0, 0, 0, 1, 0, 1, 1, 0, 0, 0, 1, 1, 1, 0, 1, 
            0, 1, 1, 0, 0, 1, 0, 1, 1, 0, 0, 1, 1, 1, 1, 0, 
            0, 0, 1, 1, 1, 1, 1, 0, 1, 1, 1, 0, 1, 0, 0, 0, 
            0, 0, 1, 1, 0, 1, 0, 1, 1, 0, 1, 1, 0, 1, 1, 1, 
            0, 1, 1, 0, 0, 0, 0, 0, 1, 0, 1, 1, 0, 1, 0, 1, 
            1, 1, 1, 1, 0, 1, 0, 1, 0, 1, 0, 1, 0, 0, 0, 0, 
            0, 0, 1, 0, 1, 0, 0, 1, 0, 1, 0, 1, 1, 1, 1, 0, 
            0, 1, 0, 1, 1, 1, 0, 1, 1, 1, 0, 0, 0, 0, 0, 0, 
            1, 1, 1, 0, 0, 1, 1, 1, 0, 1, 0, 0, 1, 0, 0, 1, 
            1, 1, 1, 0, 1, 0, 1, 1, 1, 0, 1, 0, 1, 0, 0, 0, 
            1, 0, 0, 1, 0, 0, 0, 0, 1, 1, 0, 0, 1, 1, 1, 0, 
            0, 0, 0, 1, 0, 1, 1, 1, 1, 0, 1, 1, 0, 1, 1, 0, 
            0, 1, 1, 0, 1, 0, 0, 0, 0, 1, 1, 1, 0, 1, 1, 1, 
            1, 0, 0, 0, 0, 1, 1, 1, 1, 1, 1, 1, 1, 1, 0, 0, 
            0, 0, 0, 1, 1, 1, 1, 0, 1, 1, 1, 1, 1, 0, 0, 0, 
            1, 0, 1, 1, 1, 0, 0, 1, 1, 0, 0, 1, 0, 0, 0, 0, 
            0, 1, 0, 0, 1, 0, 1, 0, 0, 1, 1, 1, 0, 1, 1, 0, 
            1, 0, 0, 0, 1, 1, 1, 1, 0, 0, 1, 1, 1, 1, 1, 0, 
            0, 1, 1, 0, 1, 1, 0, 0, 0, 1, 0, 1, 0, 1, 0, 0, 
            1, 0, 0, 0, 1, 1, 1, 0, 0, 0, 1, 1, 0, 1, 1, 0, 
            1, 0, 1, 0, 1, 1, 1, 0, 0, 0, 1, 0, 0, 1, 1, 0, 
            0, 0, 1, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
        private static byte[] poly68 = new byte[] { 
            1, 1, 1, 1, 1, 0, 0, 0, 0, 0, 0, 1, 1, 1, 1, 0, 
            0, 0, 0, 0, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 0, 0, 
            0, 0, 0, 1, 1, 1, 0, 0, 0, 0, 0, 0, 0, 1, 1, 1, 
            1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 1, 1, 1, 1, 
            1, 0, 0, 0, 1, 1, 1, 1, 1, 1, 0, 0, 0, 0, 1, 1, 
            1, 1, 1, 1, 1, 1, 1, 0, 0, 0, 0, 0, 0, 0 };
        private static byte[] poly465 = new byte[] { 
            1, 1, 0, 0, 0, 1, 1, 0, 1, 1, 1, 1, 0, 1, 1, 1, 
            1, 1, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 1, 0, 
            0, 0, 0, 1, 1, 0, 1, 0, 0, 0, 0, 1, 1, 1, 1, 1, 
            0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 1, 1, 0, 0, 0, 1, 
            1, 1, 1, 0, 1, 0, 1, 1, 1, 1, 1, 1, 1, 1, 0, 0, 
            0, 0, 0, 1, 1, 1, 1, 1, 0, 0, 0, 0, 0, 1, 1, 1, 
            1, 0, 1, 0, 1, 1, 1, 1, 1, 1, 1, 1, 0, 0, 0, 0, 
            1, 1, 0, 0, 0, 0, 0, 0, 0, 0, 1, 1, 1, 0, 0, 0, 
            1, 0, 1, 1, 1, 1, 1, 1, 1, 0, 0, 0, 0, 1, 1, 0, 
            0, 0, 0, 0, 0, 0, 1, 1, 1, 1, 1, 0, 1, 1, 1, 0, 
            1, 1, 1, 1, 1, 1, 1, 0, 0, 0, 0, 1, 0, 0, 0, 0, 
            1, 1, 1, 1, 1, 1, 1, 1, 0, 0, 1, 0, 0, 0, 1, 1, 
            1, 1, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 1, 1, 1, 
            1, 1, 1, 1, 0, 0, 0, 1, 1, 0, 1, 1, 1, 1, 1, 1, 
            0, 0, 0, 0, 0, 0, 1, 1, 0, 0, 1, 1, 1, 1, 0, 0, 
            0, 0, 0, 1, 1, 1, 0, 0, 1, 1, 1, 1, 1, 1, 0, 0, 
            0, 0, 0, 0, 1, 0, 0, 0, 1, 1, 1, 0, 0, 1, 1, 1, 
            1, 1, 0, 0, 0, 1, 1, 1, 1, 1, 1, 1, 0, 0, 0, 1, 
            1, 1, 1, 0, 0, 0, 1, 1, 0, 0, 1, 1, 0, 0, 0, 0, 
            0, 1, 1, 1, 1, 1, 1, 1, 1, 1, 0, 0, 0, 1, 0, 0, 
            0, 0, 0, 1, 1, 1, 0, 1, 1, 0, 0, 1, 1, 1, 1, 1, 
            1, 1, 1, 1, 1, 1, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 
            0, 1, 1, 1, 0, 1, 0, 0, 1, 1, 1, 1, 1, 1, 1, 1, 
            1, 1, 1, 1, 0, 0, 0, 0, 0, 1, 0, 0, 1, 1, 1, 1, 
            1, 0, 0, 1, 0, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 
            1, 0, 0, 0, 0, 0, 0, 1, 0, 0, 1, 1, 1, 1, 1, 0, 
            1, 1, 0, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 0, 0, 0, 
            0, 0, 0, 1, 1, 1, 0, 0, 1, 1, 0, 0, 0, 0, 1, 0, 
            0, 1, 1, 1, 1, 1, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 
            0, 0 };

        static byte[][] polys = new byte[][] {
                poly0, poly4, poly4, poly465,
                poly1, poly1, poly2, poly5,
                poly9, poly5, poly2, poly0,
                poly1, poly1, poly2, poly68 };

        private static int[] divisors = new int[] { 1, 1, 15, 1, 1, 1, 1, 1, 1, 1, 1, 1, 3, 3, 3, 1 };

        public const CallingConvention CALLING_CONVENTION = CallingConvention.Cdecl;

        [UnmanagedFunctionPointer(CALLING_CONVENTION)]
        public unsafe delegate void sdlAudioCallback(
                    void* userdata,
                    void* stream, int len);

        public struct SDL_AudioSpec
        {

            public int freq;
            public short format;
            public byte channels;
            public byte silence;
            public short samples;
            public short padding;
            public int size;
            public sdlAudioCallback callback;
            public object userdata;
        }

        // --- CHANNEL PROTOTYPE CLASS ---
        
        public class AudioChannel
        {
            private int m_OutputRate;
            private int m_InputRate;
            const int BaseDivider = 114;

            // Output control variables
            private int m_Divider;
            private int m_Frequency;
            private int m_SampleLength;
            private byte[] m_PolyForm;
            private byte m_Volume;
            
            // Serialization variables
            private int m_DividerTick;
            private int m_PolyOffset;            
            private int m_Output;
            private int m_RateError;

            // Audio buffer
            private Queue<byte> m_AudioOutput;

            public AudioChannel(int outputRate)
            {
                m_OutputRate = outputRate;

                AUDC(0);
                AUDV(0);
                AUDF(0);

                m_AudioOutput = new Queue<byte>();
            }

            public void SetInputRate( int inputRate )
            {
                m_InputRate = inputRate;
            }

            public void Tick()
            {
                if (++m_DividerTick >= m_SampleLength)
                {
                    m_DividerTick = 0;

                    if (m_PolyOffset >= m_PolyForm.Length)
                        m_PolyOffset = 0;

                    m_Output = m_PolyForm[m_PolyOffset++];
                }

                m_RateError += m_OutputRate;

                while (m_RateError >= m_InputRate)
                {
                    m_AudioOutput.Enqueue((byte)(m_Output * m_Volume));
                    m_RateError -= m_InputRate;
                }                
            }

            public unsafe void MixBuffer( byte* stream, int len )
            {
                byte* lastStream = stream;

                if (len > m_AudioOutput.Count)
                    return;

                while( len > 0 )
                {
                    *(stream++) += m_AudioOutput.Dequeue();
                    len--;
                }
            }

            public void AUDC(byte data)
            {
                m_PolyForm = polys[data & 0xF];
                m_Divider = divisors[data & 0xF];

                m_SampleLength = m_Divider * m_Frequency;
            }

            public void AUDF(byte data)
            {
                // 1 - 32 clock divisions
                m_Frequency = (data & 0x1f) + 1;
                m_SampleLength = m_Divider * m_Frequency;
            }

            public void AUDV(byte data)
            {
                m_Volume = (byte)((data & 0xF)<<2);
            }
        }

        // --- MIXER MAIN BODY ---

        static private int m_SampleRate;
        static private byte m_BaseLine;
        static private List<AudioChannel> m_Channels;
        static private SDL_AudioSpec m_ObtainedSpecs;

        static AudioMixer()
        {
            m_Channels = new List<AudioChannel>();            
        }

        public static unsafe void StartMixer(int sampleRate, short bufferLen)
        {
            SDL_AudioSpec desired = new SDL_AudioSpec();
            m_ObtainedSpecs = new SDL_AudioSpec();

            desired.channels = 1;
            desired.format = Sdl.AUDIO_U8;
            desired.freq = sampleRate;
            desired.samples = bufferLen;
            desired.callback = audioCallback;

            IntPtr desiredPtr = Marshal.AllocHGlobal(Marshal.SizeOf(desired));
            IntPtr obtainedPtr = Marshal.AllocHGlobal(Marshal.SizeOf(m_ObtainedSpecs));

            try
            {
                Marshal.StructureToPtr(desired, desiredPtr, false);
                int i = Sdl.SDL_OpenAudio(desiredPtr, obtainedPtr);
                m_ObtainedSpecs = (SDL_AudioSpec)Marshal.PtrToStructure(obtainedPtr, typeof(SDL_AudioSpec));
            }
            finally
            {
                Marshal.FreeHGlobal(desiredPtr);
                Marshal.FreeHGlobal(obtainedPtr);
            }

            m_SampleRate = m_ObtainedSpecs.freq;
            m_BaseLine = (byte)m_ObtainedSpecs.silence;

            Sdl.SDL_PauseAudio(0);
        }

        public static void CloseMixer()
        {
            Sdl.SDL_CloseAudio();
        }

        public static unsafe void audioCallback(void* userdata, void* buffer, int len)
        {
            byte* stream = (byte*)buffer;
            foreach (AudioChannel ch in m_Channels)
                ch.MixBuffer(stream, len);
        }

        public static AudioChannel OpenChannel()
        {
            AudioChannel ch = new AudioChannel(m_SampleRate);
            m_Channels.Add(ch);
            return ch;
        }

        public static void ReleaseChannel(AudioChannel ch)
        {
            m_Channels.Remove(ch);
        }
    }
}
