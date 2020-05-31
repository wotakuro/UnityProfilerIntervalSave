# UnityProfileIntervalSave
Unity5.6からProfilerのログを保存するツール

## IntervalSaveについて
こちらはUnity5.6から使えるツールになります。<br />
Unity5.6より、ProfilerWindowに表示された内容をファイルに書き出すことが可能となりました。<br />
この機能は、「UnityEditorInternal.ProfilerDriver.SaveProfile」というAPIで呼び出す事が可能になっています<br />
<br />
定期的にProfilerWindowの情報を監視して、300フレーム(履歴限界数)を超える前に、その時点のProfilerWindowの状態を逐次ファイルに書き出していきます。<br />
こうすることで、Profilerの結果を保存することが可能になっています。<br />

## 利用方法
MenuよりTools->UTJ->IntervalProfilerSave で、このツールを呼び出してください。ウィンドウが出てくるはずです。<br/>
※このツールはProfilerWindowが有効な状態でないと利用できません。

![Alt text](/Documentation~/img/IntervalRecordMode.png)

1).ログを書き出すディレクトリの指定をしてください<br />
2).モード切替を行います。Recorderが結果をファイルに逐次書き出すモードです。<br />
   Recorderモードになっていると、Profilerの状況を監視し、勝手にファイルを書き出すようになっています。<br />
   Viewerは書き出したファイルを見るためのものです<br />
3).プロファイラの状態をクリアします。<br />
4).現在、Profilerがつながっている対象のPlayerです<br />
5).直前に保存したファイルを書き出します。<br />

## 書き出したログの見方

![Alt text](/Documentation~/img/IntervalViewMode.png)

1).Workingディレクトリ内にあるファイルをリストアップします<br />
2).指定したファイルをProfilerへ読み込ませます<br />

