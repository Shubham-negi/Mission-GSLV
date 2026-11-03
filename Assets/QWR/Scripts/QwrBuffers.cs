/************************************************************************************

Copyright   :   Copyright 2015-2017 QWR Interactive Solutions Pvt. Ltd. All Rights reserved.

QWR Developer Website: http://developer.qwr.asia

************************************************************************************/

using System;
using System.Runtime.InteropServices;
using UnityEngine;

namespace qwr
{
	/// <summary>
	/// Manages an Qwr head-mounted display (HMD).
	/// </summary>
	public class Buffers
	{
	    private RenderTexture[] _textures = new RenderTexture[_texture_count];
		private IntPtr[] _texture_ptrs = new IntPtr[_texture_count];

		private const int _texture_count = Common.NUM_BUFFER * (int)qwrcEYE.NUM;
		private int _current_texture_index = 0;
		private int _next_texture_index = 0;
		
		/// <summary>
		/// Initialization Buffers. Called by QwrManager.
		/// </summary>
		public void Init
			( TEXTURE_DEPTH texture_depth
             , RenderTextureFormat texture_format
			 , int texture_w , int texture_h
			 )
		{
			for (int i = 0; i < _texture_count; i += 2)
			{
				ConfigureEyeTexture( i, qwrcEYE.LEFT
				                    , texture_depth
				                    , texture_format
				                    , texture_w , texture_h );
				ConfigureEyeTexture( i, qwrcEYE.RIGHT
				                    , texture_depth
				                    , texture_format
				                    , texture_w , texture_h );
			}
		}

		/// <summary>
		/// Updates the internal state of the Buffers. Called by QwrManager.
		/// </summary>
		public bool SwapBuffers()
		{
			bool ret = true;
			for (int i = 0; i < _texture_count; i++)
			{
				if (!_textures[i].IsCreated())
				{
					_textures[i].Create();
					_texture_ptrs[i] = _textures[i].GetNativeTexturePtr();
					ret = false;
				}
			}
			
			_current_texture_index = _next_texture_index;
			_next_texture_index = (_next_texture_index + 2) % _texture_count;

			return ret;
		}

		/// <summary>
		/// Gets the currently active render texture for the given eye.
		/// </summary>
		public RenderTexture GetEyeTexture(qwrcEYE eye) { return _textures[_current_texture_index + (int)eye]; }

		/// <summary>
		/// Gets the currently active render texture's native ID for the given eye.
		/// </summary>
		public IntPtr GetEyeTexturePtr(qwrcEYE eye) { return _texture_ptrs[_current_texture_index + (int)eye]; }

        // this is used for unity 5.6
        //public IntPtr GetEyeTexturePtr56(qwrcEYE eye) { return _texture_ptrs[(_current_texture_index + _texture_count + (int)eye - 2) % _texture_count]; }

        private void ConfigureEyeTexture
			( int buffer_index, qwrcEYE eye
			 , TEXTURE_DEPTH texture_depth
			 , RenderTextureFormat texture_format
			 , int texture_w , int texture_h
			 )
		{
			int tex_index = buffer_index + (int)eye;

			_textures[tex_index] = new RenderTexture
				( texture_w , texture_h
				 , (int)texture_depth, texture_format);

			if (QualitySettings.antiAliasing != 0)
			{
				_textures[tex_index].antiAliasing = QualitySettings.antiAliasing;
			}
			else
			{
			#if UNITY_ANDROID && !UNITY_EDITOR
				_textures[tex_index].antiAliasing = 4;
			#else
				_textures[tex_index].antiAliasing = 1;
			#endif
			}

			_textures[tex_index].Create();

			_texture_ptrs[tex_index] = _textures[tex_index].GetNativeTexturePtr();
		}

        public void clear ()
        {
            for (int i = 0; i < _textures.Length; i++)
            {
                if (_textures[i].IsCreated())
                {
                    _textures[i].Release();
                    _textures[i] = null;
                    _texture_ptrs[i] = IntPtr.Zero;
                }
            }
        }
	}
}