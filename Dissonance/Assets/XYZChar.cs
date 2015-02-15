using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class XYZChar : WorldEntity {
	[SerializeField]
	WorldEntity2D _xyComponent;
	[SerializeField]
	WorldEntity2D _zyComponent;

	private bool _registered = false;
	private List<IntVector> _bottomParts;

	bool IsVisible {
		get {
			bool ysMatch = _xyComponent.Location[1] == _zyComponent.Location[1];
			if (!ysMatch) { return false; }
			// foreach (var x in Beneath) {
			// 	Debug.Log(x);
			// }
			// Debug.Log("B");
			return (Beneath.Length == 1 && Beneath[0] != null);
		}
	}

	WorldEntity[] Beneath {
		get {
			HashSet<WorldEntity> beneath = new HashSet<WorldEntity>();
			for (int i = 0; i < _bottomParts.Count; i++) {
				var underneath = WorldManager.g.ContentsAt(ComputedLocation + _bottomParts[i] + new IntVector(0,-1,0));
				beneath.Add(underneath);
			}
			return beneath.ToArray();
		}
	}

	private void RegisterMe () {
		if (!_registered) {
			WorldManager.g.RegisterEntity(this);
			_registered = true;
		}
	}

	private void DeregisterMe () {
		if (_registered) {
			WorldManager.g.DeregisterEntity(this);
			_registered = false;
		}
	}

	void Start () {
		var xyLocs = _xyComponent.AbsoluteLocations(new IntVector2D());
		var zyLocs = _xyComponent.AbsoluteLocations(new IntVector2D());
		HashSet<IntVector> xyzLocs = new HashSet<IntVector>();

		int lowestY = (int)Mathf.Pow(2, sizeof(int));
		var identityLocations = new List<IntVector>();
		for (int i = 0; i < xyLocs.Count; i++) {
			for (int j = 0; j < zyLocs.Count; j++) {
				xyzLocs.Add(new IntVector(xyLocs[i].x, xyLocs[i].y, zyLocs[j][0]));
				lowestY = Mathf.Min(lowestY, xyLocs[i].y);
			}
		}
		_bottomParts = new List<IntVector>();
		foreach (var pt in xyzLocs) {
			identityLocations.Add(pt);
			if (pt.y == lowestY) {
				_bottomParts.Add(pt);
			}
		}
		SetIdentityLocations(identityLocations);


		RegisterMe();
	}

	// _loc
	// List<IntVector> _identityLocations = new List<IntVector>();

	public override bool CastsShadows {
		get { return false; }
	}

	IntVector ComputedLocation {
		get {
			return new IntVector(_xyComponent.Location[0], _xyComponent.Location[1], _zyComponent.Location[0]);
		}
	}

	void Update () {
		if (IsVisible) {
			RegisterMe();
		} else {
			DeregisterMe();
		}
	}
	public override void Simulate () {
		_loc = ComputedLocation;
	}
}
