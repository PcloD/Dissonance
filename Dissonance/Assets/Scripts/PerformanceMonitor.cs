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
	private  float refreshFrequency = 0.5f;
	[SerializeField]
	private int decimalPrecision = 1;
	#endregion

	#region Classwide hidden variables
	private float divisor = 10f; // Optimization of Mathf.Pow (10f, decimalPrecision)
	private float accum = 0f; // FPS accumulated over the interval
	private float msaccum = 0f; // ms accumulated over the interval
	private int frames = 0; // Frames drawn over the interval
	private Color fpsColor = Color.white; // Depends on the FPS ( R < 10, Y < 30, G >= 30 )
	private Color spikeColor = Color.white;
	private string fpsString = "";
	private string spikeString = "";
	private StringBuilder stringBuilder = new StringBuilder (10);
	#endregion

	#region Unity overrides
	void Start ()
	{
		StartCoroutine (FPS ());
		//StartCoroutine (TestSpike ()); // Uncomment to experiment with artificial frame spikes
	}

	void OnValidate ()
	{
		decimalPrecision = Mathf.Clamp (decimalPrecision, 0, 10);
		divisor = Mathf.Pow (10f, decimalPrecision);
	}

	float last = 0f;
	float curr = 0f;
	float biggestSpike = 0f;
	void Update ()
	{
		curr = Time.deltaTime * 1000f;
		biggestSpike = Mathf.Max (biggestSpike, Mathf.Abs (curr - last));
		accum += Time.timeScale / Time.deltaTime;
		msaccum += curr - last;
		++frames;
		last = curr;
	}

	void OnGUI () // TODO: Think about moving this into a text mesh to avoid impacting performance as much (apparently, OnGUI is very slow)
	{
		GUI.color = fpsColor;
		GUI.Label (new Rect (Screen.width - 125, Screen.height - 25, 125, 25), fpsString);
		GUI.color = spikeColor;
		GUI.Label (new Rect (Screen.width - 125, Screen.height - 45, 125, 25), spikeString);
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
			yield return new WaitForSeconds (refreshFrequency);
			float fps = accum / frames;
			float ms = msaccum / frames;
			stringBuilder.Length = 0;
			stringBuilder.Append ("ms: ");
			stringBuilder.Append (Mathf.RoundToInt (msaccum));
			stringBuilder.Append (" (");
			stringBuilder.Append (Mathf.RoundToInt (fps * divisor) / divisor);
			stringBuilder.Append (" FPS)");
			fpsString = stringBuilder.ToString ();

			stringBuilder.Length = 0;
			stringBuilder.Append ("Spike (ms): ");
			stringBuilder.Append (Mathf.RoundToInt (biggestSpike * divisor) / divisor);
			spikeString = stringBuilder.ToString ();

			fpsColor = (fps >= 30f) ? Color.green : ((fps > 10) ? Color.yellow : Color.red);
			spikeColor = (biggestSpike <= 20f) ? Color.green : ((biggestSpike < 25f) ? Color.yellow : Color.red);

			accum = 0.0F;
			frames = 0;

			biggestSpike = 0f;
		}
	}
	#endregion
}