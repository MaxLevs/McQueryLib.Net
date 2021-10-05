using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCQueryLib.Utills
{
	class ByteCounter
	{
		private byte[] _countUnits;

		public ByteCounter()
		{
			_countUnits = new byte[4];
			Reset();
		}

		public bool GetNext(byte[] receiver)
		{
			for (int i = 0; i < _countUnits.Length; ++i)
			{
				if(_countUnits[i] < 0x0F)
				{
					_countUnits[i]++;
					_countUnits.CopyTo(receiver, 0);
					return true;
				}
				else
				{
					_countUnits[i] = 0x00;
				}
			}

			return false;
		}

		public void Reset()
		{
			for(int i = 0; i < _countUnits.Length; ++i)
			{
				_countUnits[i] = 0;
			}
		}
	}
}
