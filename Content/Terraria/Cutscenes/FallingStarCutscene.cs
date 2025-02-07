using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;

using static ReverieMusic.ReverieMusic;
using Reverie.Core.Dialogue;
using Reverie.Core.Graphics;

using Reverie.Common.Systems.Camera;
using Terraria.Audio;
using Terraria.ModLoader;
using Terraria.GameContent;
using System.Linq;
using Reverie.Core;
using System.Threading.Tasks;
using Reverie.Common.Extensions;

namespace Reverie.Content.Terraria.Cutscenes
{
    public class FallingStarCutscene : Cutscene
    {
        private enum CutscenePhase
        {
            FadeIn,
            GuideScene,
            PlayerDescent,
            Impact,
            Finish
        }

        private const float FADE_DURATION = 8f;
        private const float GUIDE_SCENE_DURATION = 7f;
        private const float DESCENT_DURATION = 3f;
        private const float IMPACT_HOLD_DURATION = 5f;

        private float _elapsedTime;
        private CutscenePhase _currentPhase;
        private NPC _guide;
        private Vector2 _playerStartPosition;
        private Vector2 _playerTargetPosition;
        private bool _dialoguePlayed;
        private bool _impactTriggered;

        private int _guideDirection;
        private Vector2 _guideBottom;
        public override void Start()
        {
            base.Start();

            _elapsedTime = 0f;
            _currentPhase = CutscenePhase.FadeIn;
            _dialoguePlayed = false;
            _impactTriggered = false;

            FadeColor = Color.Black;
            FadeAlpha = 1f;
            DisableFallDamage();
            EnableInvisibility();
            SetMusic(MusicID.WindyDay);
            _guide = Main.npc[NPC.FindFirstNPC(NPCID.Guide)];
            if (_guide == null)
            {
                int guideIndex = NPC.NewNPC(default, Main.spawnTileX * 16, Main.spawnTileY * 16, NPCID.Guide);
                _guide = Main.npc[guideIndex];
            }

            _playerStartPosition = _guide.position + new Vector2(-150, -Main.screenHeight * 1.3f);
            _playerTargetPosition = _guide.position + new Vector2(-175, 0);
        }

        private static async void PlaySoundWithDelay()
        {
            for (int i = 0; i < 10; i++)
            {
                SoundEngine.PlaySound(SoundID.Item9);
                await Task.Delay(500);
            }
        }

        private void UpdatePhase(float deltaTime)
        {
            CameraSystem.DoPanAnimation((int)17f * 60, _guide.Center - new Vector2(0, -200), _guide.Center);
            switch (_currentPhase)
            {
                case CutscenePhase.FadeIn:
                    FadeAlpha = 1f - (_elapsedTime / FADE_DURATION);
                    _guide.velocity = Vector2.Zero;
                    _guide.TownNPC_TalkState();
                    if (_elapsedTime >= FADE_DURATION)
                    {
                        _currentPhase = CutscenePhase.GuideScene;
                        _elapsedTime = 0f;
                    }
                    break;

                case CutscenePhase.GuideScene:
                    if (_elapsedTime >= GUIDE_SCENE_DURATION * 0.5f && !_dialoguePlayed)
                    {
                        EnableInvisibility();
                        PlaySoundWithDelay();
                        DialogueManager.Instance.StartDialogue(NPCDataManager.GuideData, DialogueID.CrashLanding_Cutscene, true);
                        _dialoguePlayed = true;
                        CameraSystem.shake = 3;
                    }

                    if (_elapsedTime >= GUIDE_SCENE_DURATION)
                    {
                        DisableInvisibility();

                        _currentPhase = CutscenePhase.PlayerDescent;
                        _elapsedTime = 0f;
                        Main.LocalPlayer.Center = _playerStartPosition;
                    }
                    break;

                case CutscenePhase.PlayerDescent:
                    float descentProgress = _elapsedTime / DESCENT_DURATION;
                    Main.LocalPlayer.position = Vector2.Lerp(_playerStartPosition, _playerTargetPosition, descentProgress * 1.2f);
                    CameraSystem.MoveCameraOut(0, Main.LocalPlayer.Center);
                    DisableFallDamage();
                    Main.LocalPlayer.fullRotation += 1.4f * descentProgress * 1.12f;
                    FadeAlpha = descentProgress / 16;
                    if (Main.LocalPlayer.TouchedTiles.Any())
                    {
                        if (!_impactTriggered)
                        {
                            int proj = Projectile.NewProjectile(null, Main.LocalPlayer.Center, Vector2.Zero,
                                ModContent.ProjectileType<ExplosiveLanding>(), 0, 0f, Main.myPlayer);
                            _impactTriggered = true;
                            FadeColor = Color.White;
                            FadeAlpha = 1f;
                            CameraSystem.shake = 30;
                        }
                        _currentPhase = CutscenePhase.Impact;
                        _elapsedTime = 0f;
                        DialogueManager.Instance.StartDialogue(NPCDataManager.GuideData, DialogueID.CrashLanding_Intro, true);
                    }
                    break;

                case CutscenePhase.Impact:
                    float fadeProgress = _elapsedTime / IMPACT_HOLD_DURATION;
                    FadeAlpha -= fadeProgress / 16;
                    if (_elapsedTime >= IMPACT_HOLD_DURATION)
                    {
                        _currentPhase = CutscenePhase.Finish;
                    }
                    break;
            }
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
            DisablePlayerMovement();
            if (!IsPlaying) return;

            float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;
            _elapsedTime += deltaTime;

            UpdatePhase(deltaTime);
        }

        public override bool IsFinished()
        {
            return _currentPhase == CutscenePhase.Finish;
        }

        public override void End()
        {
            DialogueManager.Instance.StartDialogue(NPCDataManager.GuideData, DialogueID.CrashLanding_SettlingIn);
            EnableFallDamage();
            EnablePlayerMovement();
            CameraSystem.Reset();
            base.End();
        }
    }

    public class ExplosiveLanding : ModProjectile
    {
        private const int DefaultWidthHeight = 15;
        private const int ExplosionWidthHeight = 80;
        public override string Texture => "Terraria/Images/Star_4";
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.Explosive[Type] = true;
        }

        public override void SetDefaults()
        {
            Projectile.width = DefaultWidthHeight;
            Projectile.height = DefaultWidthHeight;
            Projectile.friendly = true;
            Projectile.penetrate = -1;

            Projectile.timeLeft = 5;

            DrawOffsetX = -2;
            DrawOriginOffsetY = -5;
        }

        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            Projectile.timeLeft = 0;
            Projectile.PrepareBombToBlow();

            if (Projectile.soundDelay == 0)
            {
                SoundEngine.PlaySound(SoundID.Item14);
            }
            Projectile.soundDelay = 10;

            return false;
        }

        public override void AI()
        {
            if (Projectile.owner == Main.myPlayer && Projectile.timeLeft <= 3)
            {
                Projectile.PrepareBombToBlow();
            }
        }

        public override void PrepareBombToBlow()
        {
            Projectile.tileCollide = false;
            Projectile.alpha = 255;
            Projectile.Resize(ExplosionWidthHeight, ExplosionWidthHeight);
        }

        public override void OnKill(int timeLeft)
        {
            SoundEngine.PlaySound(SoundID.Item14, Projectile.position);
            // Smoke Dust spawn
            for (int i = 0; i < 50; i++)
            {
                Dust dust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.Smoke, 0f, 0f, 100, default, 2f);
                dust.velocity *= 1.4f;
            }

            // Fire Dust spawn
            for (int i = 0; i < 80; i++)
            {
                Dust dust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.Torch, 0f, 0f, 100, default, 3f);
                dust.noGravity = true;
                dust.velocity *= 5f;
                dust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.Torch, 0f, 0f, 100, default, 2f);
                dust.velocity *= 3f;
            }

            // Large Smoke Gore spawn
            for (int g = 0; g < 2; g++)
            {
                var goreSpawnPosition = new Vector2(Projectile.position.X + Projectile.width / 2 - 24f, Projectile.position.Y + Projectile.height / 2 - 24f);
                Gore gore = Gore.NewGoreDirect(Projectile.GetSource_FromThis(), goreSpawnPosition, default, Main.rand.Next(61, 64), 1f);
                gore.scale = 1.5f;
                gore.velocity.X += 1.5f;
                gore.velocity.Y += 1.5f;
                gore = Gore.NewGoreDirect(Projectile.GetSource_FromThis(), goreSpawnPosition, default, Main.rand.Next(61, 64), 1f);
                gore.scale = 1.5f;
                gore.velocity.X -= 1.5f;
                gore.velocity.Y += 1.5f;
                gore = Gore.NewGoreDirect(Projectile.GetSource_FromThis(), goreSpawnPosition, default, Main.rand.Next(61, 64), 1f);
                gore.scale = 1.5f;
                gore.velocity.X += 1.5f;
                gore.velocity.Y -= 1.5f;
                gore = Gore.NewGoreDirect(Projectile.GetSource_FromThis(), goreSpawnPosition, default, Main.rand.Next(61, 64), 1f);
                gore.scale = 1.5f;
                gore.velocity.X -= 1.5f;
                gore.velocity.Y -= 1.5f;
            }
            Projectile.Resize(DefaultWidthHeight, DefaultWidthHeight);

            if (Projectile.owner == Main.myPlayer)
            {
                int explosionRadius = 7; // Bomb: 4, Dynamite: 7, Explosives & TNT Barrel: 10
                int minTileX = (int)(Projectile.Center.X / 16f - explosionRadius);
                int maxTileX = (int)(Projectile.Center.X / 16f + explosionRadius);
                int minTileY = (int)(Projectile.Center.Y / 16f - explosionRadius);
                int maxTileY = (int)(Projectile.Center.Y / 16f + explosionRadius);

                // Ensure that all tile coordinates are within the world bounds
                Utils.ClampWithinWorld(ref minTileX, ref minTileY, ref maxTileX, ref maxTileY);

                // These 2 methods handle actually mining the tiles and walls while honoring tile explosion conditions
                bool explodeWalls = Projectile.ShouldWallExplode(Projectile.Center, explosionRadius, minTileX, maxTileX, minTileY, maxTileY);
                Projectile.ExplodeTiles(Projectile.Center, explosionRadius, minTileX, maxTileX, minTileY, maxTileY, explodeWalls);
            }
        }
    }
}