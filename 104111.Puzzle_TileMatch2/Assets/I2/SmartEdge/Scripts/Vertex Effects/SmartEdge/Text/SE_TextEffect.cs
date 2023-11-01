using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;

namespace I2.SmartEdge
{
	[System.Serializable]
	public class SE_TextEffect
	{
        public SE_TextEffect_RichText _RichText           = new SE_TextEffect_RichText();
        public SE_TextEffect_TextWrapping _TextWrapping   = new SE_TextEffect_TextWrapping();
        public SE_TextEffect_Spacing _Spacing             = new SE_TextEffect_Spacing();
        public SE_TextEffect_BestFit _BestFit             = new SE_TextEffect_BestFit();


		public int ModifyVertices (SmartEdge se)
		{
            //if (_BestFit._Enabled)
            //    _BestFit.ModifyVertices(mesh, firstLayer, se);

            _RichText.ModifyVertices(se);
            _Spacing.ModifyVertices(se);
            _TextWrapping.ModifyVertices(se);

            se.UpdateCharactersMinMax(false);

            return 0;
		}		

        public bool ModifyText(SmartEdge se, ref string text)
        {
            bool modified = false;

            if (_RichText._Enabled)
                modified |= _RichText.ModifyText(ref text);

            if (_TextWrapping._Enabled)
                modified |= _TextWrapping.ModifyText(ref text);

            return modified;
        }
	}
}