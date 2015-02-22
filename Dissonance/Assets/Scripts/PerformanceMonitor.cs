using UnityEngine;
using System.Collections;
using System.Text;

// Adapted from http://wiki.unity3d.com/index.php?title=FramesPerSecond
[AddComponentMenu( "Utilities/PerformanceMonitor")]
public class PerformanceMonitor : MonoBehaviour
{
	// Attach this to any object to make a frames/second indicator.
	//
	// It calculates frames/second over each updateInterval,
	// so the display does not keep changing wildly.
	//
	// It is also fairly accurate at very low FPS counts (<10).
	// We do this not by simply counting frames per interval, but
	// by accumulating FPS for each frame. This way we end up with
	// correct overall FPS even if the interval renders something like
	// 5.5 frames.

	#region Designer-configurable variables
	[SerializeField]
	private  float _refreshFrequency = 0.5f;
	[SerializeField]
	private int _decimalPrecision = 1;
	#endregion

	#region Classwide hidden variables
	private float _divisor = 10f; // Optimization of Mathf.Pow (10f, _decimalPrecision)
	private float _accum = 0f; // FPS accumulated over the interval
	private float _msaccum = 0f; // ms accumulated over the interval
	private int _frames = 0; // Frames drawn over the interval
	private Color _fpsColor = Color.white; // Depends on the FPS ( R < 10, Y < 30, G >= 30 )
	private Color _spikeColor = Color.white;
	private string _fpsString = "";
	private string _spikeString = "";
	private StringBuilder _stringBuilder = new StringBuilder (10);
	#endregion

	#region Unity overrides
	void Start ()
	{
		StartCoroutine (FPS ());
		//StartCoroutine (TestSpike ()); // Uncomment to experiment with artificial frame spikes
	}

	void OnValidate ()
	{
		_decimalPrecision = Mathf.Clamp (_decimalPrecision, 0, 10);
		_divisor = Mathf.Pow (10f, _decimalPrecision);
	}

	float _last = 0f;
	float _curr = 0f;
	float _biggestSpike = 0f;
	void Update ()
	{
		_curr = Time.deltaTime * 1000f;
		_biggestSpike = Mathf.Max (_biggestSpike, Mathf.Abs (_curr - _last));
		_accum += Time.timeScale / Time.deltaTime;
		_msaccum += _curr;
		++_frames;
		_last = _curr;
	}

	void OnGUI () // TODO: Think about moving this into a text mesh to avoid impacting performance as much (apparently, OnGUI is very slow)
	{
		GUI.color = _fpsColor;
		GUI.Label (new Rect (Screen.width - 125, Screen.height - 25, 125, 25), _fpsString);
		GUI.color = _spikeColor;
		GUI.Label (new Rect (Screen.width - 125, Screen.height - 45, 125, 25), _spikeString);
	}
	#endregion

	#region Hidden Helper Functions
	private IEnumerator TestSpike ()
	{
		while (true) {
			yield return new WaitForSeconds (Random.Range (1f, 2f));
			BlockingSleepMilliseconds (30f); // I can notice this, but I can't really notice 25 - Julian
		}
	}

	private void BlockingSleepMilliseconds (float msToSleep)
	{
		// This will block by thrashing the CPU for the desired time, using the most accurate timer available.
		// Accuracy = 100 ns on my OS X system - Julian
		// long nanosecPerTick = (1000L * 1000L * 1000L) / System.Diagnostics.Stopwatch.Frequency;
		// Nothing else can happen in the meantime (on this thread).
		System.Diagnostics.Stopwatch stopwatch = System.Diagnostics.Stopwatch.StartNew ();
		while (true) {
			if (stopwatch.ElapsedMilliseconds >= msToSleep) {
				break;
			}
		}
		stopwatch.Stop ();
	}

	private IEnumerator FPS ()
	{
		while (true) {
			yield return new WaitForSeconds (_refreshFrequency);
			float fps = _accum / _frames;
			float ms = _msaccum / _frames;
			_stringBuilder.Length = 0;
			_stringBuilder.Append ("ms: ");
			_stringBuilder.Append (Mathf.RoundToInt(ms));
			_stringBuilder.Append (" (");
			_stringBuilder.Append (Mathf.RoundToInt (fps * _divisor) / _divisor);
			_stringBuilder.Append (" FPS)");
			_fpsString = _stringBuilder.ToString ();

			_stringBuilder.Length = 0;
			_stringBuilder.Append ("Spike (ms): ");
			_stringBuilder.Append (Mathf.RoundToInt (_biggestSpike * _divisor) / _divisor);
			_spikeString = _stringBuilder.ToString ();

			_fpsColor = (fps >= 30f) ? Color.green : ((fps > 10) ? Color.yellow : Color.red);
			_spikeColor = (_biggestSpike <= 20f) ? Color.green : ((_biggestSpike < 25f) ? Color.yellow : Color.red);

			_accum = 0.0F;
			_msaccum = 0.0f;
			_frames = 0;

			_biggestSpike = 0f;
		}
	}
	#endregion
}