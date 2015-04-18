using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class XYZChar : MonoBehaviour {
	[SerializeField]
	Char2D _xyComponent;
	[SerializeField]
	Char2D _zyComponent;
	[SerializeField]
	GameObject _visuals;
	Transform _visualsTransform;

	private List<IntVector> _bottomParts;
	private Vector3 _localAnchor;
	private WorldEntity _worldEntity;

	void Awake () {
		_worldEntity = GetComponent<WorldEntity>();
		_visualsTransform = _visuals.transform;
	}

	bool YsMatch {
		get { return _xyComponent.Location[1] == _zyComponent.Location[1]; }
	}

	bool IsVisible {
		get {
			if (!YsMatch) { return false; }
			WorldEntity[] beneath = Beneath;
			return (beneath.Length == 1 && beneath[0] != null);
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

	public Rotatable ObjectToRotate {
		get {
			if (!YsMatch) { return null; }
			WorldEntity[] beneath = Beneath;
			if (beneath.Length == 1 && beneath[0] != null) {
				return beneath[0].GetComponent<Rotatable>();
			} else {
				return null;
			}
		}
	}

	public Vector3 Anchor {
		get { return _localAnchor + _worldEntity.Location.ToVector3(); }
	}

	void Start () {
		_worldEntity.CastsShadows = false;

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
		_localAnchor = Vector3.zero;
		foreach (var pt in xyzLocs) {
			identityLocations.Add(pt);
			if (pt.y == lowestY) {
				_bottomParts.Add(pt);
				_localAnchor += pt.ToVector3();
			}
		}
		_localAnchor /= _bottomParts.Count;
		_worldEntity.SetIdentityLocations(identityLocations);

		_worldEntity.Location = ComputedLocation;

		_worldEntity.RegisterMe();
	}

	void OnEnable () {
		_worldEntity.Simulators += Simulate;
	}

	void OnDisable () {
		_worldEntity.Simulators -= Simulate;
	}

	IntVector ComputedLocation {
		get {
			return new IntVector(_xyComponent.Location[0], _xyComponent.Location[1], _zyComponent.Location[0]);
		}
	}

	void Update () {
		if (IsVisible) {
			_worldEntity.RegisterMe();
			_visuals.SetActive(true);
			Vector3 v = new Vector3(_xyComponent.VisualPos[0], _xyComponent.VisualPos[1], _zyComponent.VisualPos[0]);
			_visualsTransform.position = v;
		} else {
			_worldEntity.DeregisterMe();
			_visuals.SetActive(false);
		}
	}

	private void Simulate () {
		_worldEntity.Location = ComputedLocation;
	}
}