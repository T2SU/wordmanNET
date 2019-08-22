
// 새로운 단어를 등록. 애드-워어드
function new_word() {
    var nw = $('#new_word');

    // 입력된 단어의 좌우 양 공백 제거
    var val = nw.val().trim();

    // 입력된 단어의 길이가 0일 경우 (없을 경우) return.
    if (val.length === 0)
        return;

    // 전송할 값을 JSON 형태로 만듬.
    var dat = { "word": val };

    // AJAX 실행.
    $.ajax({
        type: 'POST', // 메서드는 POST
        url: '/add',  // 서버에 요청할 주소는 /add (start.py에 add_word() 함수를 실행할거임.
        data: dat,
        success: function (res, st, req) { // 성공시 실행될 핸들러.
            var result_data = res; // 결과를 JSON 파싱.

            var word_rows = $("#word_rows");

            // 새로운 단어 줄 생성.
            var row1 = $('<tr class="word_context"></tr>');
            var col_word = $('<th class="word"></th>');
            var col_ref = $('<td></td>');
            var col_time = $('<td class="auto-hidden"></td>');
            col_word.append(result_data['word']);
            col_ref.append(result_data['ref']);
            col_time.append(result_data['time']);
            col_word.attr('id', 'word');
            col_ref.attr('id', 'ref');
            col_time.attr('id', 'time');
            row1.append(col_word);
            row1.append(col_ref);
            row1.append(col_time);
            word_rows.prepend(row1);

            // 새로 생긴 단어 줄에 단어 세부 항목을 열기 위한 이벤트 핸들러 등록.
            row1.click(word_detail_open);

            // 새로 생긴 단어 쪽으로 스크롤링..
            $('html, body').animate({
                scrollTop: row1.offset().top
            }, 250);
            $('#new_word').val('');
        },
        error: function (res, st, req) {
            if (res.status === 400) {
                alert('이미 있는듯?');

                // 이미 단어가 등록되어 있을 경우.
                // 우선 생각중인건 에러메시지가 뜨는 대신 자동으로 이미 등록되어 있는 해당 단어를 찾아가주는 기능을 만들면 편할듯.

            } else {
                alert('error?');
            }
        }
    });
}

// 에드 워어드 버튼 이벤트 핸들러 등록
$('#add_word').click(new_word);

// 에드 워어드 텍스트 박스의 키보드 이벤트 핸들러 등록.
$('#new_word').keypress(function (e) {
    if (e.which === 13) {
        new_word();
    }
});


function collapse(ths) {
    // 임시 저장 값에 저장된 새롭게 만든 테이블 행을 불러온다.
    var new_row = ths.data('new_row');

    // 새롭게 삽입되었던 행을 fade 효과로 사라지게 하고, 다 사라지면 태그 삭제.
    // new_row.fadeOut(250, function () {
    new_row.remove();
    // });

    // 지금 이 세부 항목은 닫혀 있다고 나타내기 위한 임시 저장 값 저장.
    ths.data('mode', 'summary');
}

// 단어 세부 항목을 열었을때 처리.
function word_detail_open(event) {

    // 단어 세부 항목이 열려 있지 않은 상태이다. 세부 항목을 열어야 한다.
    if ($(this).data('mode') !== 'detail') {

        // 열린걸 우선 모두 닫아버리기.
        $('.word_context').each(function (i, e) {
            if ($(this).data('mode') === 'detail') {
                collapse($(this));
            }
        });

        // 클릭된 태그 하위에서 id가 word인걸 찾아서 그 내용을 word 변수에 저장한다.
        var word = $(this).find("#word").html();
        var ref_column = $(this).find("#ref");
        var time_column = $(this).find("#time");

        // 새로운 줄을 끼워 넣을거다.
        var new_row = $('<tr class="word_detail"><td></td></tr>').insertAfter(this);
        var new_td = new_row.find("td");
        new_td.attr('colspan', '3');

        // 부트스트랩의 card 클래스 사용. 그래야 이쁘게 나온다.
        var card = $('<div class="card"></div>');

        // card 내부에 사용되는 부트스트랩의 card-body 클래스 사용.
        // examples, synonyms, antonyms 각각 다른 card-body를 쓸거다.
        var examples_body = $('<div class="card-body"></div>');
        var synonyms_body = $('<div class="card-body"></div>');
        var antonyms_body = $('<div class="card-body"></div>');

        // 커다랗게 h4로 제목을 붙인다.
        examples_body.append('<h4 class="card-title">Examples</h4>');
        synonyms_body.append('<h4 class="card-title">Synonyms</h4>');
        antonyms_body.append('<h4 class="card-title">Antonyms</h4>');

        var examples_ul = $('<ul></ul>');
        var synonyms_ul = $('<ul></ul>');
        var antonyms_ul = $('<ul></ul>');

        // insert_form : 새로운 예문을 추가하는 텍스트박스 및 버튼 바로 전에 넣을거니까 그 텍스트박스 및 버튼.
        // value : 내용
        // detail_type : 서버에 요청할 정보 타입. 'examples', 'synonyms' 등
        var create_new_word_detail_item = function (insert_form, value, detail_type) {
            var item_li = $('<li></li>');
            item_li.html(value);
            item_li.insertBefore(insert_form);

            // 예문을 클릭했을때, 편집 입력창으로 바뀌는 핸들러 함수이다.
            var edit_handler = function (evtobj) {
                var t = $(this);

                // 처리중? 처리중이면 안함.
                var processing = t.data('processing');
                if (processing === 'true') {
                    return;
                }

                var v = t.data('mode');
                if (v !== 'editing') {
                    // 텍스트 내용을 ctx 변수에 저장. 및 'oldvalue' 임시 저장 값으로 저장.
                    var ctx = t.html();
                    t.data('oldvalue', ctx);

                    // 편집 입력창 태그임.
                    var edit_textbox = $('<input type="text" class="form-control form-control-sm mr-sm-1"/>');

                    // 편집 입력창 내용에 ctx 변수에 저장된 내용으로 넣기.
                    edit_textbox.val(ctx);

                    // 텍스트 내용 대신에 편집 입력창으로 대체.
                    t.html(edit_textbox);

                    // 편집 입력창에 포커싱.
                    edit_textbox.focus();

                    // 입력 포커싱 해제 핸들러에서 쓰기 위한 임시 저장 값 저장.
                    t.data('mode', 'editing');
                    t.data('textbox', edit_textbox);
                    t.data('context', t);

                    // 편집 입력창의 포커싱이 해제되었을때 처리할 이벤트 핸들러.
                    var edit_completed_handler = function () {
                        var textbox = t.data('textbox');
                        var context = t.data('context');

                        // 임시 저장 값 삭제.
                        t.data('mode', 'normal');
                        t.data('textbox', null);
                        t.data('context', null);

                        // var edited = 새로운 값
                        var edited = textbox.val();

                        // 만들었던 편집 입력창 삭제.
                        textbox.remove();

                        // 수정되었는지 검사. 수정되지 않았으면 그냥 유지
                        var oldvalue = t.data('oldvalue');
                        if (oldvalue !== edited) {
                            // 편집 입력창의 내용 (edited) 로 편집 입력창 대신 대체.
                            context.html('(Processing..)');

                            save_word_detail(t, edited, insert_form, detail_type);
                        } else {
                            t.html(oldvalue);
                        }

                    };

                    // 편집 입력창에 포커싱 해제 이벤트 핸들러 등록 및 키 이벤트 핸들러 등록.
                    // 키 이벤트는 포커싱 해제 대신 엔터키를 쳐도 포커싱 해제처럼 이벤트 핸들러가 실행되게 하려고.
                    edit_textbox.focusout(edit_completed_handler);
                    edit_textbox.keypress(function (e) {
                        if (e.which === 13) {
                            edit_completed_handler();
                        }
                    });
                }
            };

            // 위에 정의된, 항목 클릭시 편집 텍스트로 바뀌는 핸들러를 항목들에 클릭 이벤트 핸들러로 등록한다.
            item_li.click(edit_handler);
        };

        // 예문 등 정보를 받아올 AJAX 실행.
        // insert_form : 새로운 예문을 추가하는 텍스트박스 및 버튼 바로 전에 넣을거니까 그 텍스트박스 및 버튼.
        // detail_type : 서버에 요청할 정보 타입. 'examples', 'synonyms' 등
        var request_word_detail = function (insert_form, detail_type) {
            var dat = { "detail_type": detail_type, "word": word };
            $.ajax({
                type: 'POST', // 메서드는 POST
                url: '/get_detail',  // 서버에 요청할 주소는 /add (start.py에 add_word() 함수를 실행할거임.
                data: dat,
                success: function (res, st, req) { // 성공시 실행될 핸들러.
                    var result_data = res; // 결과를 JSON 파싱.
                    for (var row in result_data) {
                        create_new_word_detail_item(insert_form, result_data[row], detail_type);
                    }
                },
                error: function (res, st, req) {
                }
            });
        };


        // 예문 등 정보를 서버에다가 저장할 AJAX 실행.
        // item : <li></li> 태그의 jquery.
        // newvalue : 저장할 새로운 정보.
        // insert_form : 새로운 예문을 추가하는 텍스트박스 및 버튼 바로 전에 넣을거니까 그 텍스트박스 및 버튼.
        // detail_type : 서버에 요청할 정보 타입. 'examples', 'synonyms' 등
        var save_word_detail = function (item, newvalue, insert_form, detail_type) {
            if (!item.length && !newvalue.length)
                return;
            if (item.length) {
                if (item.data('processing') === 'true')
                    return;
                item.data('processing', 'true');
            }

            dat = { "word": word, "detail_type": detail_type };

            // <li></li> 태그 jquery가 발견되어 전달되었다면 해당 jquery에서 'oldvalue' 저장 값 가져오기.
            if (item.length) {
                dat['oldone'] = item.data('oldvalue');
            } else {
                dat['oldone'] = '';
            }

            dat['newone'] = newvalue;

            $.ajax({
                type: 'POST', // 메서드는 POST
                url: '/change_detail',  // 서버에 요청할 주소는 /add (start.py에 add_word() 함수를 실행할거임.
                data: dat,
                success: function (res, st, req) { // 성공시 실행될 핸들러.
                    if (res === 'new') {
                        create_new_word_detail_item(insert_form, newvalue, detail_type);
                    } else if (res === 'del') {
                        item.fadeOut(250, function () {
                            item.remove();
                        });
                    } else if (res === 'mod') {
                        item.html(newvalue);
                    }
                    if (item.length) {
                        item.data('processing', '');
                    }
                },
                error: function (res, st, req) {
                    alert('서버에 저장하는 작업이 실패하였습니다.');
                }
            });
        };

        // 참조 횟수, 시간 갱신
        var update_ref = function () {
            $.ajax({
                type: 'POST', // 메서드는 POST
                url: '/update_ref',  // 서버에 요청할 주소는 /update_ref
                data: { "word": word },
                success: function (res, st, req) { // 성공시 실행될 핸들러.
                    ref_column.html(res['ref']);
                    time_column.html(res['time']);
                },
                error: function (res, st, req) {
                    alert('참조 갱신 실패');
                }
            });
        };

        // 서버에 정보 요청 전에 우선 열려있다고 처리.
        // 지금 이 세부 항목은 열려 있다고 나타내기 위한 임시 저장 값 저장.
        $(this).data('mode', 'detail');

        // 나중에 세부 항목을 닫을 때, 새롭게 만들어진 테이블 행을 삭제하기 위해서, 새롭게 만든 테이블 행을 임시 저장 값에 저장.
        $(this).data('new_row', new_row);

        // 새로운 예문 등을 등록하는 텍스트 박스 및 버튼.
        var new_example_form = $('<li><div class="form-inline min-100 min-table"><div class="form-group max-100"><div class="min-table-row w-100"><div class="min-table-cell w-100 pr-1"><input type="text" class="form-control form-control-sm mr-sm-1 w-100" placeholder="New Example..." id="new_example_textbox"/></div><div class="min-table-cell"><button class="btn btn-sm btn-info my-1 px-4 max-btn" id="new_example_btn">Add</button></div></div></div></div></li>');
        examples_ul.append(new_example_form);
        var new_synonym_form = $('<li><div class="form-inline min-100 min-table"><div class="form-group max-100"><div class="min-table-row w-100"><div class="min-table-cell w-100 pr-1"><input type="text" class="form-control form-control-sm mr-sm-1 w-100" placeholder="New Synonym..." id="new_synonym_textbox"/></div><div class="min-table-cell"><button class="btn btn-sm btn-info my-1 px-4 max-btn" id="new_synonym_btn">Add</button></div></div></div></div></li>');
        synonyms_ul.append(new_synonym_form);
        var new_antonym_form = $('<li><div class="form-inline min-100 min-table"><div class="form-group max-100"><div class="min-table-row w-100"><div class="min-table-cell w-100 pr-1"><input type="text" class="form-control form-control-sm mr-sm-1 w-100" placeholder="New Antonym..." id="new_antonym_textbox"/></div><div class="min-table-cell"><button class="btn btn-sm btn-info my-1 px-4 max-btn" id="new_antonym_btn">Add</button></div></div></div></div></li>');
        antonyms_ul.append(new_antonym_form);

        // ul을 card-body에 추가.
        examples_body.append(examples_ul);
        synonyms_body.append(synonyms_ul);
        antonyms_body.append(antonyms_ul);


        // 세부 내용 본문 (card 클래스를 가졌던 div 레이어) 에 예문, 유의어, 반의어 태그를 추가할 것임.
        card.append(examples_body);
        card.append(synonyms_body);
        card.append(antonyms_body);

        // 새롭게 삽입된 테이블 행에 세부 내용 본문을 넣을 것임.
        new_td.append(card);

        // 이벤트 등록 [예문]
        $('#new_example_btn').click(function () {
            save_word_detail('', $('#new_example_textbox').val(), new_example_form, 'example');
            $('#new_example_textbox').val('');
        });
        $('#new_example_textbox').keypress(function (e) {
            if (e.which === 13) {
                save_word_detail('', $('#new_example_textbox').val(), new_example_form, 'example');
                $('#new_example_textbox').val('');
            }
        });
        // 이벤트 등록 [유의어]
        $('#new_synonym_btn').click(function () {
            save_word_detail('', $('#new_synonym_textbox').val(), new_synonym_form, 'synonym');
            $('#new_synonym_textbox').val('');
        });
        $('#new_synonym_textbox').keypress(function (e) {
            if (e.which === 13) {
                save_word_detail('', $('#new_synonym_textbox').val(), new_synonym_form, 'synonym');
                $('#new_synonym_textbox').val('');
            }
        });
        // 이벤트 등록 [반의어]
        $('#new_antonym_btn').click(function () {
            save_word_detail('', $('#new_antonym_textbox').val(), new_antonym_form, 'antonym');
            $('#new_antonym_textbox').val('');
        });
        $('#new_antonym_textbox').keypress(function (e) {
            if (e.which === 13) {
                save_word_detail('', $('#new_antonym_textbox').val(), new_antonym_form, 'antonym');
                $('#new_antonym_textbox').val('');
            }
        });

        // 서버에 정보 요청
        request_word_detail(new_example_form, 'example');
        request_word_detail(new_synonym_form, 'synonym');
        request_word_detail(new_antonym_form, 'antonym');
        update_ref();

        // 이제 새롭게 삽입된 테이블 행을 fade 효과로 나타나게 함.
        new_row.fadeIn(250, function () {
            new_row.show();
        });
    }


    // 여기는 단어 세부 항목이 열려 있는 상태이다. 닫아야 한다.
    else {
        collapse($(this));
    }
}

// 단어들을 클릭시 세부 항목이 열리게끔 이벤트 핸들러들을 등록.
$('.word_context').click(word_detail_open);