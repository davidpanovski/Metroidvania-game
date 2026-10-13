$ErrorActionPreference = 'Stop'
$source = Get-Content (Join-Path $PSScriptRoot '../Assets/Scripts/PlayerController.cs') -Raw
function Extract-Method([string]$name) {
    $match = [regex]::Match($source, '(?:private\s+)?void\s+' + $name + '\s*\(')
    if (!$match.Success) { throw "Missing method: $name" }
    $start = $source.IndexOf('{', $match.Index)
    $depth = 1
    $end = $start + 1
    while ($depth -gt 0) {
        if ($source[$end] -eq '{') { $depth++ }
        if ($source[$end] -eq '}') { $depth-- }
        $end++
    }
    return $source.Substring($match.Index, $end - $match.Index)
}
$methods = (Extract-Method 'Hit') + (Extract-Method 'Recoil') + (Extract-Method 'StopRecoilX') + (Extract-Method 'StopRecoilY')
# Execute the production methods with controlled overlap results, without Unity physics.
$code = @'
using System;
public struct Vector2 {
 public float x,y;
 public Vector2(float x,float y) { this.x=x; this.y=y; }
 public static Vector2 up { get { return new Vector2(0,1); } }
 public static Vector2 down { get { return new Vector2(0,-1); } }
}
public class Transform {
 public Vector2 position;
 public Transform parent;
 public bool IsChildOf(Transform other) { return this == other || (parent != null && parent.IsChildOf(other)); }
}
public class Rigidbody2D { public Vector2 linearVelocity; public float gravityScale; }
public class Enemy { public void EnemyHit(float d,Vector2 dir,float s) {} }
public class Collider2D {
 public Transform transform = new Transform();
 public Rigidbody2D attachedRigidbody;
 public T GetComponent<T>() where T:class { return null; }
 public bool CompareTag(string tag) { return false; }
}
public static class Physics2D {
 public static Collider2D[] hits;
 public static Collider2D[] OverlapBoxAll(Vector2 p,Vector2 a,float angle,int mask) { return hits; }
}
public static class Debug { public static void Log(string s) {} }
public class State { public bool recoilingX,recoilingY,lookingRight=true; }
public class RecoilRegression {
 Rigidbody2D rb = new Rigidbody2D();
 Transform transform = new Transform();
 State pState = new State();
 float yAxis, recoilYDirection, damage, Mana, manaGain;
 float gravity=9.5f,recoilxSpeed=100,recoilYSpeed=32,recoilXSteps=5,recoilYSteps=5;
 int attackableLayer=64,StepsXrecoiled,StepsYrecoiled,airJumpsCounter;
 bool isGrounded() { return false; }
 public static int Run() {
  int failures=0;
  var p = new RecoilRegression();
  p.yAxis=-1;
  Physics2D.hits=new [] {new Collider2D {transform=p.transform,attachedRigidbody=p.rb}};
  p.Hit(p.transform,new Vector2(3.5f,4.5f),ref p.pState.recoilingY,Vector2.down,32);
  p.Recoil();
  Check("Self overlap must not launch player",p.rb.linearVelocity.y==0,ref failures);
  p=new RecoilRegression();
  Physics2D.hits=new [] {new Collider2D()};
  p.yAxis=-1;
  p.Hit(p.transform,new Vector2(3.5f,4.5f),ref p.pState.recoilingY,Vector2.down,32);
  p.yAxis=0; // Release down before next physics tick.
  p.Recoil();
  Check("Down hit bounces up after releasing input",p.rb.linearVelocity.y==32,ref failures);
  p=new RecoilRegression();
  p.Hit(p.transform,new Vector2(3.5f,4.5f),ref p.pState.recoilingY,Vector2.up,32);
  p.yAxis=-1; // Changing input must not reverse an up hit.
  p.Recoil();
  Check("Up hit stays downward after input changes",p.rb.linearVelocity.y==-32,ref failures);
  p=new RecoilRegression();
  Physics2D.hits=new Collider2D[0];
  p.Hit(p.transform,new Vector2(3.5f,4.5f),ref p.pState.recoilingY,Vector2.down,32);
  p.Recoil();
  Check("Miss does not launch player",p.rb.linearVelocity.y==0,ref failures);
  return failures;
 }
 static void Check(string name,bool pass,ref int failures) {
  Console.WriteLine((pass ? "PASS: " : "FAIL: ")+name);
  if(!pass) failures++;
 }
'@
Add-Type -TypeDefinition ($code + $methods + '}') -WarningAction SilentlyContinue
$failures = [RecoilRegression]::Run()
if ($failures -gt 0) { throw "$failures recoil regressions failed" }
