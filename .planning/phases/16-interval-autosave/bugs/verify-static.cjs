// Run from the repository root: node .planning/phases/16-interval-autosave/bugs/verify-static.cjs
// BUG-010: preserve raw line counts; separately inspect executable lines and committed scope.
const fs=require('fs'), cp=require('child_process');
const baseline='34306e3f7305a4d3f9b457282291bb94a515b7a6';
const timer='Assets/SaveSystem/Script/AutoSaveTimer.cs',manager='Assets/SaveSystem/Script/SaveLoadManager.cs',notice='Assets/SaveSystem/Script/AutoSaveNotice.cs';
const read=p=>fs.readFileSync(p,'utf8').replace(/\r\n/g,'\n');
const git=(...a)=>cp.execFileSync('git',a,{encoding:'utf8',stdio:['ignore','pipe','ignore']}).trim();
const count=(p,pattern,fixed=false)=>{
 const args=['--no-heading','-n',...(fixed?['-F']:[]),pattern,p,...(p==='Assets'?['-g','*.cs']:[])];
 const r=cp.spawnSync('rg',args,{encoding:'utf8'});if(r.status>1)throw Error(r.stderr);
 return (r.stdout||'').trim().split('\n').filter(Boolean).length;
};
const rows=[];
const add=(id,decision,command,actual,expected,adjustment)=>rows.push({id,decision,command,actual,expected,rawPass:JSON.stringify(actual)===JSON.stringify(expected),adjustment});
const strip=s=>s.replace(/\/\/[^\n]*/g,'');
const ct=strip(read(timer)),cm=strip(read(manager));
add(1,'D-01','rg SaveSlotDialog timer',count(timer,'SaveSlotDialog'),0,{actual:ct.includes('SaveSlotDialog'),expected:false,reason:'주석 제외 실제 참조'});
add(2,'D-01','rg SelectSlot timer',count(timer,'SelectSlot'),0);
add(3,'D-01','rg _auto.json Assets -g *.cs',count('Assets','_auto.json'),0);
add(4,'D-02','rg \\.bak Assets -g *.cs',count('Assets','\\.bak'),0);
add(5,'D-03','rg -F Playing gate timer',count(timer,'state.CurrentState != GameStateManager.GameState.Playing',true),1);
add(6,'D-03b','rg GameStateManager|CurrentState manager',count(manager,'GameStateManager|CurrentState'),0);
add(7,'D-03b','rg -F spawn clear guard manager',count(manager,'if (_data.SceneName != currentScene) _data.SpawnPointName = "";',true),1);
add(8,'D-03b','git diff --numstat baseline -- manager',git('diff','--numstat',baseline,'--',manager),`8\t0\t${manager}`);
add(9,'D-03c','rg -F player presence guard timer',count(timer,'return PlayerStats.Instance != null;',true),1);
add(10,'D-04','rg -F interval constant timer',count(timer,'public const float IntervalSeconds = 180f;',true),1);
add(11,'D-05','rg -F reset hook manager',count(manager,'AutoSaveTimer.NotifySaveWritten();',true),1);
add(12,'D-05','rg NotifySaveWritten Assets -g *.cs',count('Assets','NotifySaveWritten'),2,{actual:(ct+'\n'+cm).split('\n').filter(l=>l.includes('NotifySaveWritten')).length,expected:2,reason:'선언+호출만 계수, 주석 제외'});
add(13,'D-05b','rg isInCombat|inCombat|IsInCombat Assets -g *.cs',count('Assets','isInCombat|inCombat|IsInCombat'),0);
add(14,'D-06','git diff --numstat baseline -- SaveData.cs',git('diff','--numstat',baseline,'--','Assets/SaveSystem/Script/SaveData.cs'),'');
add(15,'D-07','rg -F direct SaveAnywhere call timer',count(timer,'SaveLoadManager.Instance.SaveAnywhere();',true),1);
add(16,'D-07','rg SaveAtCheckpoint|ResetHealthToMax timer',count(timer,'SaveAtCheckpoint|ResetHealthToMax'),0,{actual:/SaveAtCheckpoint|ResetHealthToMax/.test(ct),expected:false,reason:'주석 제외 실제 호출'});
add(17,'D-07b/c','rg direct Save()|SaveAuto timer',count(timer,'SaveLoadManager.Instance.Save()',true)+count(timer,'SaveAuto'),0);
add(18,'D-10','rg -F Korean message notice',count(notice,'자동 저장됨',true),1);
add(19,'D-10','rg -F Show call timer',count(timer,'if (notice != null) notice.Show();',true),1);
add(20,'D-11','git diff --numstat baseline -- settings files',git('diff','--numstat',baseline,'--','Assets/SaveSystem/Script/SettingsData.cs','Assets/Player/Script/Menu/GameSettingsPanel.cs'),'');
const expectedFiles=[notice,notice+'.meta',timer,timer+'.meta',manager].sort();
add(21,'범위','git diff --name-only baseline -- Assets',git('diff','--name-only',baseline,'--','Assets').split('\n').filter(Boolean).length,5,{actual:git('diff','--name-only',baseline,'HEAD','--','Assets').split('\n').filter(Boolean).sort(),expected:expectedFiles,reason:'기존 미커밋 변경 제외, baseline..HEAD 코드 커밋 범위'});
const build=cp.spawnSync('dotnet',['build','Assembly-CSharp.csproj','--nologo','-v','quiet','--no-incremental'],{encoding:'utf8'});
const warningLines=[...new Set((build.stdout||'').split('\n').filter(l=>l.includes(': warning ')).map(l=>l.trim()))];
add(22,'빌드','dotnet build Assembly-CSharp.csproj --nologo -v quiet --no-incremental',{exit:build.status,warnings:warningLines.length},{exit:0,warnings:6});
const extra={
 baseline,
 baselineIsFirstTaskParent:git('rev-parse','b05668b^')===baseline,
 rawSaveAnywhereLines:count('Assets','SaveAnywhere'),
 executableSaveAnywhereLines:[timer,manager,'Assets/Player/Script/Menu/GameSettingsPanel.cs'].reduce((n,p)=>n+strip(read(p)).split('\n').filter(l=>l.includes('SaveAnywhere')).length,0),
 noticeRawProhibitedLines:count(notice,'GraphicRaycaster|EventSystem|HealPopup|Resources.Load|SerializeField'),
 noticeExecutableProhibited:/GraphicRaycaster|EventSystem|HealPopup|Resources.Load|SerializeField/.test(strip(read(notice))),
 wireDelta:git('show','--format=','--numstat','05e4dec','--',timer),
 noticeTypeLines:count(timer,'AutoSaveNotice'),
 noticeTypeOccurrences:(read(timer).match(/AutoSaveNotice/g)||[]).length,
 warningLines
};
const result={rows,extra,rawPassed:rows.filter(r=>r.rawPass).length,adjustedPassed:rows.filter(r=>r.adjustment?JSON.stringify(r.adjustment.actual)===JSON.stringify(r.adjustment.expected):r.rawPass).length};
console.log(JSON.stringify(result,null,2));
if(result.adjustedPassed!==22)process.exitCode=1;
