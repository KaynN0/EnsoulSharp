 using EnsoulSharp;
using EnsoulSharp.SDK;
using EnsoulSharp.SDK.MenuUI;
using EnsoulSharp.SDK.MenuUI.Values;
using EnsoulSharp.SDK.Prediction;
using EnsoulSharp.SDK.Utility;
using SPrediction;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace Danz0r_AIO.Champions
{
    /*  _____                         
  * |     |___ ___ ___ ___ ___ ___ 
  * | | | | . |  _| . | .'|   | .'|
  * |_|_|_|___|_| |_  |__,|_|_|__,|
  *               |___|            
  *               
  * Revision:  
  * + Lag free drawings
  * + Current target is now red (was white)
  * 
  * Revision:  
  * + Possible Q Hitchance Fix
  * + Add Hitchance Extreme in Q
  * Revision: 
  * + Imporved Q Prediction
  * + Combo Key added
  * + BlackShield Distance checks
  * + Can choose hitchance in menu now
  * 
  * Revision 
  * + Menu Updates
  * 
  * Rivision 
  * + Should no walk to a target to cast W

  * Revision:
  * + Release
  */

    class Morgana
    {
        public static Menu _Menu = null;

        public static int MenubufferQ = 0;
        public static int MenubufferW = 0;
        public static int MenubufferE = 0;
        public static int MenubufferR = 0;
        public static int MenubufferColQ = 0;
        public static int MenubufferColW = 0;
        public static int MenubufferColE = 0;
        public static int MenubufferColR = 0;

        private static Spell q, w, e, r;
        private static SpellSlot ignite;
        private static AIHeroClient _player { get { return ObjectManager.Player; } }
        #region
        //Combat
        public static readonly MenuBool comboQ = new MenuBool("comboQ", "Q Combo");
        public static readonly MenuBool comboW = new MenuBool("comboW", "W Combo");
        public static readonly MenuBool comboE = new MenuBool("comboE", "E only Stun Combo");
        public static readonly MenuBool comboR = new MenuBool("comboR", "R Combo");
        public static readonly MenuSlider comboQRange = new MenuSlider("qrange", "Pick the Q Range", 1175, 1, 1175);
        public static readonly MenuSlider comboWRange = new MenuSlider("wrange", "Pick the W Range", 900, 1, 900);
        public static readonly MenuSlider comboERange = new MenuSlider("erange", "Pick the E Range", 800, 1, 800);
        public static readonly MenuSlider comboRRange = new MenuSlider("rrange", "Pick the R Range", 625, 1, 625);
        public static readonly MenuList qmodus = new MenuList<string>("qmodus", "Q Spell Logic", new[] { "Skillshot", "Targetted", "Self Cast/Buff" });
        public static readonly MenuList wmodus = new MenuList<string>("wmodus", "W Spell Logic", new[] { "Skillshot", "Targetted", "Self Cast/Buff" });
        public static readonly MenuList emodus = new MenuList<string>("emodus", "E Spell Logic", new[] { "Skillshot", "Targetted", "Self Cast/Buff" });
        public static readonly MenuList rmodus = new MenuList<string>("rmodus", "R Spell Logic", new[] { "Skillshot", "Targetted", "Self Cast/Buff" });
        
        //antigap
        public static readonly MenuBool AntiGapq = new MenuBool("AntiGapq", "Use Q on gapcloser");
        public static readonly MenuBool AntiGapw = new MenuBool("AntiGapw", "Use W on gapcloser");
        public static readonly MenuBool AntiGape = new MenuBool("AntiGape", "Use E on gapcloser");
        public static readonly MenuBool AntiGapr = new MenuBool("AntiGapr", "Use R on gapcloser");

        //Laneclear
        public static readonly MenuBool laneQ = new MenuBool("laneQ", "Use Q on Clear Wave");
        public static readonly MenuBool laneW = new MenuBool("laneW", "Use W on Clear Wave");
        
        //Collision Manager
        public static readonly MenuBool ColQ = new MenuBool("ColQ", "Q spell has Collision?");
        public static readonly MenuBool ColW = new MenuBool("ColW", "W spell has Collision?");
        public static readonly MenuBool ColE = new MenuBool("ColE", "E spell has Collision?");
        public static readonly MenuBool ColR = new MenuBool("ColR", "R spell has Collision?");

        //KS
        public static readonly MenuBool useKS = new MenuBool("useKS", "Enable KS?");
        public static readonly MenuBool qKS = new MenuBool("qKS", "KS with Q");
        public static readonly MenuBool wKS = new MenuBool("wKS", "KS with W");
        //jungle clear
        public static readonly MenuBool JungleQ = new MenuBool("JungleQ", "Use Q ");
        public static readonly MenuBool JungleW = new MenuBool("JungleW", "Use W ");
        public static readonly MenuBool JungleE = new MenuBool("JungleE", "Use E ");
        //Harrass
        public static readonly MenuBool HarassQ = new MenuBool("HarassQ", "Use Q ");
        public static readonly MenuBool HarassW = new MenuBool("HarassW", "Use W ");

        //Hit Chance
        public static readonly MenuList qhit = new MenuList<string>("qhit", "Q - HitChance :", new[] { "Extreme", "High", "Medium", "Low" });
        public static readonly MenuList whit = new MenuList<string>("whit", "W - HitChance :", new[] { "Extreme", "High", "Medium", "Low" });
        public static readonly MenuList rhit = new MenuList<string>("rhit", "R - HitChance :", new[] { "Extreme", "High", "Medium", "Low" });

        //Draw
        public static readonly MenuBool Qd = new MenuBool("qd", "Draw Q Range");
        public static readonly MenuBool Wd = new MenuBool("wd", "Draw W Range");
        public static readonly MenuBool Ed = new MenuBool("ed", "Draw E Range");
        public static readonly MenuBool Rd = new MenuBool("rd", "Draw R Range");
        #endregion

        public static void OnLoad()
        {
           
            
            ignite = _player.GetSpellSlot("summonerdot");
            
            MenuCreate();
            OnChange();
            Game.OnUpdate += Game_OnGameUpdate;
            Drawing.OnDraw += OnDraw;
        }

        public static void OnChange()
        {
            q = new Spell(SpellSlot.Q, comboQRange.Value);
            w = new Spell(SpellSlot.W, comboWRange.Value);
            e = new Spell(SpellSlot.E, comboERange.Value);
            r = new Spell(SpellSlot.R, comboRRange.Value);

            switch (qmodus.Index)
            {
                case 0:
                    if(ColQ == true)
                    {
                        q.SetSkillshot(0.25f, 70f, 1175f, true, SkillshotType.Line);
                    }
                    else
                    q.SetSkillshot(0.25f, 70f, 1175f, false, SkillshotType.Line);
                    break;
                case 1:

                    q.SetTargetted(0.25f, float.MaxValue);
                    break;
                default:
                     q.SetSkillshot(0.25f, 70f, 1175f, true, SkillshotType.Line);
                    break;

            }
            switch (wmodus.Index)
            {
                case 0:
                    if (ColW == true)
                    {
                        w.SetSkillshot(0.5f, 400f, 2200f, true, SkillshotType.Circle);
                    }
                    else
                        w.SetSkillshot(0.5f, 400f, 2200f, false, SkillshotType.Circle);
                    break;
                case 1:
                    w.SetTargetted(0.05f, float.MaxValue);
                    break;
                default:
                    w.SetSkillshot(0.5f, 400f, 2200f, true, SkillshotType.Circle);
                    break;

            }
            switch (emodus.Index)
            {
                case 0:
                    if (ColE == true)
                    {
                        e.SetSkillshot(0.25f, 100, 1800, true, SkillshotType.Line);
                    }
                    else
                        e.SetSkillshot(0.25f, 100, 1800, false, SkillshotType.Line);
                    break;
                case 1:
                    e.SetTargetted(0.25f, float.MaxValue);
                    break;
                default:
                    e.SetSkillshot(0.25f, 100, 1800, true, SkillshotType.Line);
                    break;

            }
            switch (rmodus.Index)
            {
                case 0:
                    if (ColR == true)
                    {
                        r.SetSkillshot(0.25f, 100, 1800, true, SkillshotType.Line);
                    }
                    else
                        r.SetSkillshot(0.25f, 100, 1800, false, SkillshotType.Line);
                    break;
                case 1:
                    r.SetTargetted(0.25f, float.MaxValue);
                    break;
                default:
                    r.SetSkillshot(0.25f, 100, 1800, true, SkillshotType.Line);
                    
                    break;

            }
           

        }
        private static void MenuCreate()
        {
            var _menu = new Menu("DANZ0R AIO", "DANZ0R_Morgana", true);
            var hitconfig = new Menu("hitconfig", "Hit-Chance Settings");
            hitconfig.Add(qhit);
            hitconfig.Add(whit);
            hitconfig.Add(rhit);

            var combat = new Menu("combat", "Combo Settings");
            combat.Add(comboQ);           
            combat.Add(comboW);
            combat.Add(comboE);
            combat.Add(comboR);
            

            var HarrasMenu = new Menu("HarrasMenuu", "Harras Settings");
            HarrasMenu.Add(HarassQ);
            HarrasMenu.Add(HarassW);


            var antiGP2 = new Menu("antiGP2", "GapClose");
            antiGP2.Add(AntiGapq);
            antiGP2.Add(AntiGapw);
            antiGP2.Add(AntiGape);
            antiGP2.Add(AntiGapr);


            var clearwave = new Menu("waveclear", "LaneClear Settings");
            clearwave.Add(laneQ);
            clearwave.Add(laneW);

            var JungleMenu = new Menu("JungleMenue", "Jungle clear");
            JungleMenu.Add(JungleQ);
            JungleMenu.Add(JungleW);
            


            var misc = new Menu("ks", "KS Settings");
            misc.Add(useKS);
            misc.Add(qKS);
            misc.Add(wKS);


            var draw = new Menu("draw", "Drawings");
            draw.Add(Qd);
            draw.Add(Wd);
            draw.Add(Ed);
            draw.Add(Rd);

            var pred = new Menu("spred", "Predict Selection");
            SPrediction.Prediction.Initialize(pred);
            
            _menu.Add(hitconfig);
            _menu.Add(combat);
            _menu.Add(HarrasMenu);
            _menu.Add(antiGP2);
            _menu.Add(clearwave);
            _menu.Add(JungleMenu);
            _menu.Add(misc);
            _menu.Add(draw);
            _menu.Add(pred);
            _menu.Attach();
        }

        public static void Game_OnGameUpdate(EventArgs args)
        {
            if (_player.IsDead)
                return;
            if (comboQRange.Interacting || comboERange.Interacting || comboRRange.Interacting || comboWRange.Interacting || MenubufferQ != qmodus.Index || MenubufferW != wmodus.Index || MenubufferE != emodus.Index || MenubufferR != rmodus.Index
                         )
            {
                
                MenubufferQ = qmodus.Index;
                MenubufferW = wmodus.Index;
                MenubufferE = emodus.Index;
                MenubufferR = rmodus.Index;
                
                OnChange();
                
                
            }

            if (useKS.Enabled)
                KS();

            switch (Orbwalker.ActiveMode)
            {
                case OrbwalkerMode.Combo:
                    DoCombo();
                    break;
                case OrbwalkerMode.Harass:
                    DoHarass();
                    break;
                case OrbwalkerMode.LaneClear:
                    DoLaneClear();
                    DoJungleClear();
                    break;
                    
            }
        }

        private static void OnGapcloser(AIHeroClient sender, Gapcloser.GapcloserArgs args)
        {
            if (ObjectManager.Player.IsDead || ObjectManager.Player.IsRecalling())
            {
                return;
            }

            if (MenuGUI.IsChatOpen || MenuGUI.IsShopOpen)
            {
                return;
            }
            
            try
            {

                if (q.IsReady() && sender.IsValidTarget(q.Range) && AntiGapq)
                {
                    if(qmodus.Index <= 1)
                    {

                        q.SPredictionCast(sender, HitChance.Medium);
                    }
                else
                    q.Cast();
                }

                if (w.IsReady() && sender.IsValidTarget(w.Range) && AntiGapw)
                {
                    if (wmodus.Index <= 1)
                    {
                        w.SPredictionCast(sender, HitChance.Medium);
                    }
                    else
                        w.Cast();
                }
                if (e.IsReady() && sender.IsValidTarget(e.Range) && AntiGape)
                {
                    if (emodus.Index <= 1)
                    {
                        e.SPredictionCast(sender, HitChance.Medium);
                    }
                    else
                        e.Cast();
                }
                if (r.IsReady() && sender.IsValidTarget(r.Range) && AntiGapr)
                {
                    if (rmodus.Index <= 1)
                    {
                        r.SPredictionCast(sender, HitChance.Medium);
                    }
                    else
                        r.Cast();
                }
            }
            catch
            {
                //error
            }
        }

       

        private double getComboDamage(AIHeroClient target)
        {
            double damage = _player.GetAutoAttackDamage(target);
            if (q.IsReady() && comboQ.Enabled)
                damage += _player.GetSpellDamage(target, SpellSlot.Q);
            if (w.IsReady() && comboW.Enabled)
                damage += _player.GetSpellDamage(target, SpellSlot.W);
            if (e.IsReady() && comboE.Enabled)
                damage += _player.GetSpellDamage(target, SpellSlot.E);
            if (r.IsReady() && comboR.Enabled)
                damage += _player.GetSpellDamage(target, SpellSlot.R);
            if (ignite.IsReady())
                damage += _player.GetSummonerSpellDamage(target, SummonerSpell.Ignite);
            return damage;
        }


        public static HitChance hitchanceW()
        {
            var hit = HitChance.High;
            switch (whit.Index)
            {
                case 0:
                    hit = HitChance.High;
                    break;
                case 1:
                    hit = HitChance.Medium;
                    break;
                case 2:
                    hit = HitChance.Low;
                    break;
                case 3:
                    hit = hit = HitChance.VeryHigh;
                    break;
            }
            return hit;
        }

        public static HitChance hitchanceE()
        {
            var hit = HitChance.High;
            switch (whit.Index)
            {
                case 0:
                    hit = HitChance.High;
                    break;
                case 1:
                    hit = HitChance.Medium;
                    break;
                case 2:
                    hit = HitChance.Low;
                    break;
                case 3:
                    hit = hit = HitChance.VeryHigh;
                    break;
            }
            return hit;
        }
        public static HitChance hitchanceQ()
        {
            var hit = HitChance.High;
            switch (qhit.Index)
            {
                case 0:
                    hit = HitChance.High;
                    break;
                case 1:
                    hit = HitChance.Medium;
                    break;
                case 2:
                    hit = HitChance.Low;
                    break;
                case 3:
                    hit = hit = HitChance.VeryHigh;
                    break;
            }
            return hit;
        }
        public static HitChance hitchanceR()
        {
            var hit = HitChance.High;
            switch (rhit.Index)
            {
                case 0:
                    hit = HitChance.High;
                    break;
                case 1:
                    hit = HitChance.Medium;
                    break;
                case 2:
                    hit = HitChance.Low;
                    break;
                case 3:
                    hit = hit = HitChance.VeryHigh;
                    break;
            }
            return hit;
        }

        private static void DoCombo()
        {
            
            var qtarget = TargetSelector.GetTarget(q.Range);
            var wtarget = TargetSelector.GetTarget(w.Range);
            var etarget = TargetSelector.GetTarget(e.Range);
            var rtarget = TargetSelector.GetTarget(r.Range);
            var hitQ = hitchanceQ();
            var hitW = hitchanceW();
            var hitR = hitchanceR();

            if (comboQ && q.IsReady() && qtarget.IsValidTarget(q.Range))
            {
                switch (qmodus.Index)
                {
                    case 0:
                        q.SPredictionCast(qtarget, hitQ);
                        break;
                    case 1:
                        q.Cast(qtarget);
                        break;
                    case 2:
                        q.Cast();
                        break;
                }
            }
            if (comboW && w.IsReady() && wtarget.IsValidTarget(w.Range))
            {
                switch (wmodus.Index)
                {
                    case 0:
                        w.SPredictionCast(wtarget, hitW);
                        break;
                    case 1:
                        w.Cast(wtarget);
                        break;
                    case 2:
                        w.Cast();
                        break;
                }
            }
            if (comboE && e.IsReady() && etarget.IsValidTarget(e.Range))
            {
                switch (emodus.Index)
                {
                    case 0:

                        break;
                    case 1:
                        e.Cast(etarget);
                        break;
                    case 2:
                        e.Cast();
                        break;
                }
            }
            if (comboR && r.IsReady() && rtarget.IsValidTarget(r.Range))
            {
                switch (rmodus.Index)
                {
                    case 0:
                        r.SPredictionCast(rtarget, hitR);
                        break;
                    case 1:
                        r.Cast(rtarget);
                        break;
                    case 2:
                        r.Cast();
                        break;
                }
            }




        }

        private static void DoHarass()
        {

            var qtarget = TargetSelector.GetTarget(q.Range);
            var wtarget = TargetSelector.GetTarget(w.Range);
            var etarget = TargetSelector.GetTarget(e.Range);
            
            var hitQ = hitchanceQ();
            var hitW = hitchanceW();
            var hitE = hitchanceE();
            

            if (HarassQ && q.IsReady() && qtarget.IsValidTarget(q.Range))
            {
                switch (qmodus.Index)
                {
                    case 0:
                        q.SPredictionCast(qtarget, hitQ);
                        break;
                    case 1:
                        q.Cast(qtarget);
                        break;
                    case 2:
                        q.Cast();
                        break;
                }
            }
            if (HarassW && w.IsReady() && wtarget.IsValidTarget(w.Range))
            {
                switch (wmodus.Index)
                {
                    case 0:
                        w.SPredictionCast(wtarget, hitW);
                        break;
                    case 1:
                        w.Cast(wtarget);
                        break;
                    case 2:
                        w.Cast();
                        break;
                }
          
                switch (emodus.Index)
                {
                    case 0:
                        e.SPredictionCast(etarget, hitE);
                        break;
                    case 1:
                        e.Cast(etarget);
                        break;
                    case 2:
                        e.Cast();
                        break;
                }
            }
        }

        private static void DoLaneClear()
        {
            var qminions = GameObjects.EnemyMinions.Where(x => x.IsValidTarget(q.Range) && x.IsMinion()).Cast<AIBaseClient>().ToList();
            var wminions = GameObjects.EnemyMinions.Where(x => x.IsValidTarget(w.Range) && x.IsMinion()).Cast<AIBaseClient>().ToList();
            var eminions = GameObjects.EnemyMinions.Where(x => x.IsValidTarget(e.Range) && x.IsMinion()).Cast<AIBaseClient>().ToList();
            var qfarm = q.GetLineFarmLocation(qminions);
            var wfarm = w.GetCircularFarmLocation(wminions);
            var efarm = e.GetLineFarmLocation(eminions);
            var Qfarm = qminions.FirstOrDefault();
            var Wfarm = wminions.FirstOrDefault();
            var Efarm = eminions.FirstOrDefault();

            if (qminions.Any() && laneQ && q.IsReady())
            {
                
                switch (qmodus.Index)
                {
                    case 0:
                        q.Cast(qfarm.Position);
                        break;
                    case 1:
                        q.Cast(Qfarm.Position);
                        break;
                    case 2:
                        q.Cast();
                        break;
                }

            }

            if (wminions.Any() && laneW && w.IsReady())
            {

                switch (wmodus.Index)
                {
                    case 0:
                        w.Cast(wfarm.Position);
                        break;
                    case 1:
                        w.Cast(Wfarm.Position);
                        break;
                    case 2:
                        w.Cast();
                        break;
                }
    

                switch (emodus.Index)
                {
                    case 0:
                        e.Cast(efarm.Position);
                        break;
                    case 1:
                        e.Cast(Efarm.Position);
                        break;
                    case 2:
                        e.Cast();
                        break;
                }
            }
        }

        private static void DoJungleClear()
        {
            var Qmob = GameObjects.Jungle.Where(x => x.IsValidTarget(q.Range) && x.GetJungleType() != JungleType.Unknown).OrderByDescending(x => x.MaxHealth).FirstOrDefault();
            var Wmob = GameObjects.Jungle.Where(x => x.IsValidTarget(w.Range) && x.GetJungleType() != JungleType.Unknown).OrderByDescending(x => x.MaxHealth).FirstOrDefault();
            var Emob = GameObjects.Jungle.Where(x => x.IsValidTarget(e.Range) && x.GetJungleType() != JungleType.Unknown).OrderByDescending(x => x.MaxHealth).FirstOrDefault();

            if (!Qmob.IsValid())
                return;

            if (Qmob.IsValidTarget(q.Range) && JungleQ && q.IsReady())
            {

                switch (qmodus.Index)
                {
                    case 0:
                        q.Cast(Qmob.Position);
                        break;
                    case 1:
                        q.Cast(Qmob);
                        break;
                    case 2:
                        q.Cast();
                        break;
                }

            }

            if (Wmob.IsValidTarget(w.Range) && JungleW && w.IsReady())
            {

                switch (wmodus.Index)
                {
                    case 0:
                        w.Cast(Wmob.Position);
                        break;
                    case 1:
                        w.Cast(Wmob);
                        break;
                    case 2:
                        w.Cast();
                        break;
                }
            }

            if (Emob.IsValidTarget(e.Range) && JungleE && e.IsReady())
            {

                switch (emodus.Index)
                {
                    case 0:
                        e.Cast(Emob.Position);
                        break;
                    case 1:
                        e.Cast(Emob);
                        break;
                    case 2:
                        e.Cast();
                        break;
                }
            }
        }
        private static void KS()
        {
            var qtarget = TargetSelector.GetTarget(q.Range);
            var wtarget = TargetSelector.GetTarget(w.Range);
            var etarget = TargetSelector.GetTarget(e.Range);
            var rtarget = TargetSelector.GetTarget(r.Range);
            if (qtarget != null && qtarget.IsValidTarget(q.Range) && qtarget.Health < q.GetDamage(qtarget) && qKS.Enabled)
            {
                if (qmodus.Index <= 1)
                {
                    q.SPredictionCast(qtarget,HitChance.Medium);
                }
                else
                    q.Cast();
            }
            if (wtarget != null && wtarget.IsValidTarget(w.Range) && wtarget.Health < w.GetDamage(wtarget) && wKS.Enabled)
            {
                if (wmodus.Index <= 1)
                {
                    w.SPredictionCast(wtarget, HitChance.Medium);
                }
                else
                    w.Cast();
          
                if (emodus.Index <= 1)
                {
                    e.SPredictionCast(etarget, HitChance.Medium);
                }
                else
                    e.Cast();
           
                if (wmodus.Index <= 1)
                {
                    r.SPredictionCast(rtarget, HitChance.Medium);
                }
                else
                    r.Cast();
            }
        }

        private static void OnDraw(EventArgs args)
        {
            if (ObjectManager.Player.IsDead || ObjectManager.Player.IsRecalling())
            {
                return;
            }

            if (MenuGUI.IsChatOpen || MenuGUI.IsShopOpen)
            {
                return;
            }
            if (Qd.Enabled && q.IsReady() || comboQRange.Interacting)
            {
                Render.Circle.DrawCircle(GameObjects.Player.Position, q.Range, Color.FromArgb(48, 120, 252), 1);
            }
            if (Wd.Enabled && w.IsReady() || comboWRange.Interacting)
            {
                Render.Circle.DrawCircle(GameObjects.Player.Position, w.Range, Color.FromArgb(120, 120, 252), 1);
            }
            if (Ed.Enabled && e.IsReady() || comboERange.Interacting)
            {
                Render.Circle.DrawCircle(GameObjects.Player.Position, e.Range, Color.FromArgb(120, 252, 252), 1);
            }
            
            if (Rd.Enabled && r.IsReady() || comboRRange.Interacting )
            {
                Render.Circle.DrawCircle(GameObjects.Player.Position, r.Range, Color.FromArgb(255, 10, 10), 1);
                    
            }
            
            
            
        }

    }
}
