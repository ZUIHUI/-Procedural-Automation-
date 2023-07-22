import doctest
import jieba
from gensim import corpora, models, similarities
from gensim.similarities import MatrixSimilarity
import pandas as pd

# 匯入關鍵字
import sys

if len(sys.argv) == 1:
    print('no argument', flush=True)
    sys.exit()
pass

temp = sys.argv[1].split(",")


# doc_test = sys.argv[1]
# doc_test = "出院"

################################
# 1. 將【推薦文句】轉成【空間座標】
################################

##########
# 1.1. 收集推薦文句

data_path = "D:/專題/程式/test/data/DART.xlsx"
dart = pd.read_excel(data_path)

##########
# 1.2. 挑出每篇【推薦文句】的關鍵字

all_doc = dart["soapier"].values.tolist()

all_doc_list = []
for doc in all_doc:
    # 用jieba找出關鍵字
    doc_list = [word for word in jieba.cut(doc)]
    all_doc_list.append(doc_list)
pass

#print("\n每篇推薦文章關鍵字: ", all_doc_list)


##########
# 1.3. 利用corpora的函示庫，編碼【推薦文章關鍵字】

dictionary = corpora.Dictionary(all_doc_list)

# 列出有多少編碼
#print("\n列出有多少編碼: ", dictionary.keys())

# 列出關鍵字與編碼的對應關係:

#print("\n列出關鍵字與編碼的對應關係: ", (dictionary.token2id))

##########
# 1.4. 將【推薦文章】轉換到空間座標
# 向量中的元素是一個二元組（編號、頻次數），對應分詞後的文檔中的每一個詞。


corpus = [dictionary.doc2bow(doc) for doc in all_doc_list]
#print("\n校正前的推薦文章空間座標: ", corpus)


##########
# 1.5. 建立tf-idf model
tfidf_model = models.TfidfModel(corpus)

##########
# 1.6. 將1.4的空間座標，利用tf-idf校正
corpus_tfidf = tfidf_model[corpus]

# print('\n利用TF-IDF模型校正後的推薦文章空間座標：')
# for i in corpus_tfidf:
#     print(i)

# 利用TF-IDF模型校正後的推薦文章空間座標：

################################
# 2. 將【測試文章】轉成【空間座標】
################################

##########
# 2.1. 收集測試文章：請寫一段目前最貼近你心情的一句話
# doc_test="我喜歡台北的小吃"
# doc_test="彈性學習課程"
##########
for i in temp:
    doc_test = i
    # 2.2. 挑出【測試文章】的關鍵字
    doc_test_list = [word for word in jieba.cut(doc_test)]
    # print("\n測試文字關鍵字: ", doc_test_list)

    ##########
    # 2.3. 利用corpora的函示庫，編碼【測試文章關鍵字】
    # 向量中的元素是一個二元組（編號、頻次數），對應分詞後的文檔中的每一個詞。
    doc_test_vec = dictionary.doc2bow(doc_test_list)
    # print("\n校正前的測試文章空間座標: ", doc_test_vec)

    ##########
    # 1.6. 將1.4的空間座標，利用tf-idf校正
    test_tfidf = tfidf_model[doc_test_vec]

    # print('\n利用TF-IDF模型校正後的測試文章空間座標：')
    # for i in test_tfidf:
    #     print(i)

    ################################
    # 3. 利用cosin similarity測試兩篇文章的相似度
    ################################

    # 建立cosin similarity的模型
    corpus_similarity_ = similarities.SparseMatrixSimilarity(
        corpus_tfidf, num_features=len(dictionary.keys()))

    # 計算【測試文章】對每個推薦文章的cosin similarity
    sim = corpus_similarity_[test_tfidf]
    # print(sim)

    # 針對similarity排序
    # sorted: 排序
    # enumerate(sim): 原來sim是個陣列[0.552131   0.02762816 0. 0.18715775 0.18715775 0.03514912  0.  0.7009363 ]
    #                 enumerate是將陣列每個數字再加上個索引值
    #                 [(0, 0.552131) (1, 0.02762816) (2,  0.) (3, 0.18715775) (4, 0.18715775) (5, 0.03514912) (6,  0.) (7, 0.7009363) ]
    # key= keyword: keyword[1]： 將enumerate(sim)的第一個元素當作是排序的依據
    # reverse=True: 由大到小排列, false是由小到大排列
    result = sorted(
        enumerate(sim), key=lambda keyword: keyword[1], reverse=True)
    # 印出
    top1 = dart.loc[[result[0][0]], ['soapier']].values.tolist()
    opt1 = ''.join(map(str, top1))
    print(opt1, flush=True)
