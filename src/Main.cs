// Copyright (c) 2017 Leacme (http://leac.me). View LICENSE.md for more information.
using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public class Main : Spatial {

	public AudioStreamPlayer Audio { get; } = new AudioStreamPlayer();

	private List<(int pIndex, Vector3[] points, Spatial beeModel)> bees = new List<(int index, Vector3[] pts, Spatial beeModel)>();

	private void InitSound() {
		if (!Lib.Node.SoundEnabled) {
			AudioServer.SetBusMute(AudioServer.GetBusIndex("Master"), true);
		}
	}

	public override void _Notification(int what) {
		if (what is MainLoop.NotificationWmGoBackRequest) {
			GetTree().ChangeScene("res://scenes/Menu.tscn");
		}
	}

	public override void _Ready() {
		var env = GetNode<WorldEnvironment>("sky").Environment;
		env.BackgroundColor = new Color(Lib.Node.BackgroundColorHtmlCode);
		env.BackgroundMode = Godot.Environment.BGMode.Sky;
		env.BackgroundSky = new PanoramaSky() { Panorama = ((Texture)GD.Load("res://assets/hill.hdr")) };
		env.BackgroundSkyRotationDegrees = new Vector3(0, 0, -25);
		// env.BackgroundEnergy = 0.5f;
		env.BackgroundSkyCustomFov = 90;
		// env.DofBlurFarEnabled = true;
		InitSound();
		AddChild(Audio);

	}

	public override void _Input(InputEvent @event) {
		if (@event is InputEventScreenTouch te && te.Pressed) {
			var p = new Path() { Curve = new Curve3D() };
			Enumerable.Range(3, (int)GD.RandRange(4, 7)).ToList().ForEach(z => p.Curve.AddPoint(new Vector3((float)GD.RandRange(-1, 1), (float)GD.RandRange(-1, 1), (float)GD.RandRange(-1, 1))));
			p.Curve.AddPoint(p.Curve.GetPointPosition(0));
			p.Curve.BakeInterval = (float)GD.RandRange(0.01f, 0.05f);
			var beeModel = new CSGSphere() { Scale = new Vector3(0.015f, 0.015f, 0.015f) };
			AddChild(beeModel);
			bees.Add((0, p.Curve.GetBakedPoints(), beeModel));
		}
	}

	public override void _Process(float delta) {
		for (int i = 0; i < bees.Count; i++) {
			if (bees[i].points.Length > 0) {
				var tempTrans = bees[i].beeModel.Transform;
				tempTrans.origin = bees[i].points[bees[i].pIndex];
				bees[i].beeModel.Transform = tempTrans;
				var tempIndex = (bees[i].pIndex < bees[i].points.Length - 1) ? bees[i].pIndex + 1 : 0;
				bees[i] = (tempIndex, bees[i].points, bees[i].beeModel);
			}
		}
	}

}
