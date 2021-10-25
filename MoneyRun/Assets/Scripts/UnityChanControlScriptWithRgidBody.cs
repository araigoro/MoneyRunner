//
// Mecanimのアニメーションデータが、原点で移動しない場合の Rigidbody付きコントローラ
// サンプル
// 2014/03/13 N.Kobyasahi
//
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using System;

namespace UnityChan
{
	public class UnityChanControlScriptWithRgidBody : MonoBehaviour
	{

		public float animSpeed = 1.5f;              // アニメーション再生速度設定
		public float lookSmoother = 3.0f;           // a smoothing setting for camera motion
		public bool useCurves = true;               // Mecanimでカーブ調整を使うか設定する
													// このスイッチが入っていないとカーブは使われない
		public float useCurvesHeight = 0.5f;        // カーブ補正の有効高さ（地面をすり抜けやすい時には大きくする）

		// キャラクターコントローラ（カプセルコライダ）の移動量
		private Vector3 velocity;
		// CapsuleColliderで設定されているコライダのHeiht、Centerの初期値を収める変数
		private float orgColHight;
		private Vector3 orgVectColCenter;
		private Animator anim;                          // キャラにアタッチされるアニメーターへの参照
		private AnimatorStateInfo currentBaseState;         // base layerで使われる、アニメーターの現在の状態の参照

		private GameObject cameraObject;    // メインカメラへの参照

		// アニメーター各ステートへの参照
		static int idleState = Animator.StringToHash("Base Layer.Idle");
		static int locoState = Animator.StringToHash("Base Layer.Locomotion");
		static int jumpState = Animator.StringToHash("Base Layer.JUMP00");
		static int restState = Animator.StringToHash("Base Layer.Rest");

		static int riseState = Animator.StringToHash("Base Layer.Rising");
		static int fallState = Animator.StringToHash("Base Layer.Falling");
		static int landState = Animator.StringToHash("Base Layer.Landing");

		//中心を0として、一番右のレーンを2、左を-2、幅を1と固定
		const int MinLane = -2;
		const int MaxLane = 2;
		const float LaneWidth = 1.0f;

		public int targetLane;

		public float h;

		CharacterController unitychan;

		Vector3 moveDirection = Vector3.zero;

		public float gravity;
		public float speedZ;
		public float speedX;
		public float speedJump;
		public float accelerationZ;
		public float colormile;
		public float colormoney;
		private float directionX;
		private float directionY;

		public int money;
		public int minusmoneytext;
		public int rate;
		public int soundnum;
		public int mile;
		public int plusmile;
		public int plusmiletext;
		private int milenum;
		public int recordscore;

		public bool gameover;
		public bool flagmile;
		public bool flagmoney;
		
		public AudioClip[] SE;
		private AudioSource audiosource;

		public Text PlusMileage;
		public Text MinusMoney;

		private Vector3 touchStartPos;
		private Vector3 touchEndPos;

		private string Direction;
		private bool isTouch;
		private bool isMove;

		// 初期化
		void Start ()
		{
			// コンポーネントの取得
			anim = GetComponent<Animator> ();
			unitychan = GetComponent<CharacterController>();
			audiosource = gameObject.GetComponent<AudioSource>();

			//スタートの所持金０円
			money = 0;

			//ゲームオーバーしないようにfalseにする
			gameover = false;
			
			//Unityちゃんのzの位置を記録
			mile = (int)transform.position.z;

			//アイテムによるスコア加算を記録
			plusmile = 0;

			plusmiletext = 0;
			flagmile = false;
			flagmoney = false;
			milenum = 0;
			
			colormile = 0;
			colormoney = 0;

			//ベストスコアを取得
			recordscore = PlayerPrefs.GetInt("score", 0);
			isTouch = false;
			isMove = false;
		}
	
	
		// 以下、メイン処理.リジッドボディと絡めるので、FixedUpdate内で処理を行う.
		void Update ()
		{
			//0以上にしているとGManagerで音が鳴り続けてしまうのでマイナス値を代入
			soundnum = -1;

			//Unityちゃんの位置＋アイテムによるスコア加算を合計して記録
			mile = (int)transform.position.z + plusmile;

			//Unityちゃんが100m進むごとの処理
			if((int)transform.position.z>(milenum+1)*100)
            {
				//前進スピードを加算
				speedZ += 0.2f;
				
				milenum += 1;
            }
		
			//アイテムによるスコアの加算が行なわれた際の処理
			if (flagmile == true)
			{
				//加算分のスコアを表示
				PlusMileage.text = "+" + plusmiletext.ToString("N0");

				//表示する際に赤色に指定（255表示だとうまくいかないので注意）
				PlusMileage.color = new Color(1,0.1462f,0.1462f, colormile);

				//だんだん色を薄くする
				colormile -= Time.deltaTime;

				if (colormile < 0)
				{
					//0つまり透明に固定
					colormile = 0;

					//処理がループしないようにfalseにする
					flagmile = false;
				}
			}

			//アイテムなどにより所持金の減少が起った際の処理
			if (flagmoney == true)
			{
				//減少分の金額を青色で表示（以下はスコア加算の場合と同じ）
				MinusMoney.text = "-" + minusmoneytext.ToString("N0");
				MinusMoney.color = new Color(0.05490198f, 0.2756866f, 0.9254902f, colormoney);
				colormoney -= Time.deltaTime;

				if (colormoney < 0)
				{
					colormoney = 0;
					flagmoney = false;
				}
			}

			anim.SetFloat ("Speed", speedZ);							// Animator側で設定している"Speed"パラメタにvを渡す
			anim.SetFloat ("Direction", h); 	// Animator側で設定している"Direction"パラメタにhを渡す
			
			anim.speed = animSpeed;								// Animatorのモーション再生速度に animSpeedを設定する
			currentBaseState = anim.GetCurrentAnimatorStateInfo (0);    // 参照用のステート変数にBase Layer (0)の現在のステートを設定する

			// 以下、キャラクターの移動処理
			//ゲームオーバーになった時
			if (gameover == true)
            {
				//Unityちゃんの動きを止める
				moveDirection.x = 0.0f;
				moveDirection.y = 0.0f;
				moveDirection.z = 0.0f;

				//1秒送らしてFailureを実行
				Invoke(nameof(Failure), 1.0f);

				//以降の処理をストップ
				enabled = false;
			}
			else
            {

			//徐々に加速しZ方向に常に前進させる
			float acceleratedZ = moveDirection.z + (accelerationZ * Time.deltaTime);
			moveDirection.z = Mathf.Clamp(acceleratedZ, 0, speedZ);

			//X方向は目標のポジションまでの差分の割合で速度を計算
			float ratioX = (targetLane * LaneWidth - transform.position.x) / LaneWidth;
			moveDirection.x = ratioX * speedX;
			 }
			
			//重力分の力を毎フレーム追加
			moveDirection.y -= gravity * Time.deltaTime;

			//移動実行
			Vector3 globalDirection = transform.TransformDirection(moveDirection);
			unitychan.Move(globalDirection * Time.deltaTime);

			//フリック対応
			if (isMove) Move();
			else Flick();

			//unityroom用キャラクター操作
			if(Input.GetKeyDown(KeyCode.RightArrow))
            {
				Direction = "right";
				isMove = true;
			}
			if (Input.GetKeyDown(KeyCode.LeftArrow))
			{
				Direction = "left";
				isMove = true;
			}
			if (Input.GetKeyDown(KeyCode.Space))
			{
				Direction = "up";
				isMove = true;
			}

			//移動後接地していたらY方向の速度をリセット
			if (unitychan.isGrounded&&Direction!="up")
			{
				moveDirection.y = 0;
			}

			//所持金マイナスでゲームオーバー
			if(money<0)
            {
				gameover = true;

				//ゲームオーバーアニメーションを動かす
				anim.SetBool("Damage2", true);

				//ゲームオーバーの音を指定
				soundnum = 3;
            }
		
			//Animationの遷移ステートがriseStateにある時
			if(currentBaseState.nameHash==riseState)
            {
				//Unityちゃんが落下している場合
				if(moveDirection.y<0)
                {
					//落ちるモーションに切り替え
					anim.SetBool("Fall", true);
                }
			    //ジャンプモーションをfalseにする
				anim.SetBool("Jump", false);
			}
			else if (currentBaseState.nameHash == fallState)
            {
				anim.SetBool("Fall", false);
				//地面に接地したとき
				if (unitychan.isGrounded)
				{
					//着地モーションに切り替え
					anim.SetBool("Land", true);
				}
			}
			else if (currentBaseState.nameHash == landState)
            {
				//接地が終わったらfalseにしておく
				anim.SetBool("Land", false);			
			}

			// REST中の処理
			// 現在のベースレイヤーがrestStateの時
			else if (currentBaseState.nameHash == restState) {
				//cameraObject.SendMessage("setCameraPositionFrontView");		// カメラを正面に切り替える
				// ステートが遷移中でない場合、Rest bool値をリセットする（ループしないようにする）
				if (!anim.IsInTransition (0)) {
					anim.SetBool ("Rest", false);
				}
			}
		}

		//右移動処理
		public void MoveToRight()
        {
			//一番右以外にいるとき
			if (targetLane < MaxLane) targetLane++;		
		}

		//左移動処理
		public void MoveToLeft()
        {
			//一番左以外にいるとき
			if (targetLane > MinLane) targetLane--;			
		}

		//ジャンプ処理
		public void Jump()
        {
			//地面に接地している場合
			if(unitychan.isGrounded)
            {
				//上方向にジャンプ力分の数値を代入
				moveDirection.y = speedJump;

				//ジャンプモーションに切り替え
				anim.SetBool("Jump",true);

				//ジャンプ音を指定
				soundnum = 0;
			}
        }
		
		//ゲームオーバーしたときに遅らせて実行したい処理のまとめ
		public void Failure()
        {
			//ベストスコアを更新したとき
			if(mile>recordscore)
            {
				//喜びのモーションに切り替え
				anim.SetBool("win2", true);

				//コルーチンの実行
				StartCoroutine(Delay(1.0f, () =>
				 {
					 //祝福の音を鳴らす
					 audiosource.PlayOneShot(SE[1]);
				 }));
            }
			else
            {
				//悔しいモーションに切り替え
				anim.SetBool("Failure", true);

				//残念音を鳴らす
				audiosource.PlayOneShot(SE[0]);
			}		
		}

		//IEnumeratorはコルーチンを実行する際のデータの型
		private IEnumerator Delay(float time, Action action)
        {
			//time秒分処理を止める
			yield return new WaitForSeconds(time);
			//action内の処理を実行
			action();
        }

		//スコア加算メソッド
		public void PlusMile(int num)
        {
			//所持金より高いモノを買っていない場合
			if(money>=0)
            {
				flagmile = true;

				//スコア加算のテキストの薄さを指定
				colormile = 0.7f;

				//加算分を記録（累計）
				plusmile += num;

				//その場でいくつ加算されたかを記録
				plusmiletext = num;
            }
        }

		//所持金減少メソッド
		public void Minusmoney(int num)
        {
			flagmoney = true;

			//所持金減少のテキストの薄さを指定
			colormoney = 0.7f;

			//所持金から引く
			money -= num;

			//いくら引かれたかを記録
			minusmoneytext = num;
        }

		//Unityちゃんが何のオブジェクトに衝突したか
        public void OnControllerColliderHit(ControllerColliderHit hit)
        {
			//衝突したオブジェクトのタグで判断
            switch(hit.gameObject.tag)
            {
				case "1dol":
					//所持金プラス
					money += rate;
					soundnum = 1;
					break;
				case "quater":
					money += rate/4;
					soundnum = 1;
					break;
				case "penny":
					money += rate/10;
					soundnum = 1;
					break;
				case "100dol":
					money += 100*rate;
					soundnum = 1;
					break;
				case "400dol":
					money += 400*rate;
					soundnum = 1;
					break;
				case "jewelry":
					money += 10000*rate;
					soundnum = 9;
					break;
				case "police":
					Minusmoney(5000000);
					soundnum = 2;
					break;
				case "chicken":
					//ゲームオーバー処理
					anim.SetBool("Damage1", true);
					gameover = true;
					soundnum = 3;
					break;
				case "bicycle":
					Minusmoney(50000);
					PlusMile(100);
					soundnum = 5;
					break;
				case "car":
					Minusmoney(500000);
					PlusMile(1500);
					soundnum = 4;
					break;
				case "sportscar":
					Minusmoney(3000000);
					PlusMile(9000);
					soundnum = 4;
					break;
				case "policecar":
					Minusmoney(5000000);
					soundnum = 2;
					break;
				case "plane":
					Minusmoney(15000000);
					PlusMile(60000);
					soundnum = 6;
					break;
				case "rateup":
					//レートをプラス
					rate += 10;
					soundnum = 7;
					break;
				case "ratedown":
					//レートをマイナス
					rate -= 10;
					soundnum = 8;
					break;
				case "tax":
					//距離が進むほど徴収される額を5万ずつ増額
					Minusmoney(100000+(PlayerPrefs.GetInt("tax",0)-1)*50000);
					soundnum = 2;
					break;

			}
			if(money>=0&&hit.gameObject.tag!="chicken"&&hit.gameObject.tag!="Ground")
            {
				//ゲームオーバーをしていない、かつニワトリ、地面以外に衝突した場合そのオブジェクトを破壊
				Destroy(hit.gameObject);				
			}

		 }

		public void Flick()
        {
			if(Input.GetKeyDown(KeyCode.Mouse0)&&isTouch==false)
            {
				//フリックの始点を記録
				touchStartPos = Input.mousePosition;
				isTouch = true;
            }
			else if(Input.GetKeyUp(KeyCode.Mouse0)&&isTouch==true)
            {
				//フリックの終点を記録
				touchEndPos = Input.mousePosition;
				isTouch = false;
				GetDirection();
			}
        }
		public void GetDirection()
        {
			//フリック位置の差分を計算して代入
			Vector3 diff = touchEndPos - touchStartPos;
			directionX = diff.x;
			directionY = diff.y;

			//横移動が大きい場合
			if (Mathf.Abs(directionY)<Mathf.Abs(directionX))
            {
				if(30<directionX)
                {
					//右フリック判定
					Direction = "right";
                }
				else if(-30>directionX)
                {
					//左フリック判定
					Direction = "left";
                }
            }
			//縦移動が大きい場合
			else if(Mathf.Abs(directionX)< Mathf.Abs(directionY))
            {
				if(30<directionY)
                {
					//上フリック判定
					Direction = "up";
                }
            }
			isMove = true;			
		}

		//移動実行
        public void Move()
        {
			switch (Direction)
			{
				case "right":
					MoveToRight();
					isMove = false;
					break;
				case "left":
					MoveToLeft();
					isMove = false;
					break;
				case "up":
					Jump();
					isMove = false;
					break;
			}
		}

    }
}