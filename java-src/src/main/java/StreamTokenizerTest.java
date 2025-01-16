import java.util.*;
import java.io.*;

public class StreamTokenizerTest {
    public static void main(String[] args) throws Exception {
        System.out.println("StreamTokenizer test");

        streamTokenizerPrintln("1");
        streamTokenizerPrintln("\"1\"");
        streamTokenizerPrintln("'1'");
        streamTokenizerPrintln("'1");
        streamTokenizerPrintln("'1\" '");
        // streamTokenizerPrintln("`1`");

        // streamTokenizerPrintln("aaaa");
        // streamTokenizerPrintln("\"aaaa");
        // streamTokenizerPrintln("\"aaaa\"");
        // streamTokenizerPrintln("\"aa aa\"");
        // streamTokenizerPrintln("'aaaa");
        // streamTokenizerPrintln("'aaaa'");
        // streamTokenizerPrintln("'aaaa\"");
        // streamTokenizerPrintln("'aaaa\" aaaa");
        // streamTokenizerPrintln("aaaa aaaa");
        // streamTokenizerPrintln("\"aaaa\" aaaa");
        // streamTokenizerPrintln("+aaaa+ aaaa");
        // streamTokenizerPrintln("select field1, field2 from table1 where field1 =
        // 'value1'");
        // streamTokenizerPrintln("hey, this is a test. how are you?");
    }

    private static void streamTokenizerPrintln(String s) throws Exception {
        System.out.println("================");
        System.out.println("Input string: " + s);
        System.out.println("----------------");
        StringReader reader = new StringReader(s);
        StreamTokenizer tokenizer = new StreamTokenizer(reader);

        while (tokenizer.ttype != StreamTokenizer.TT_EOF) {
            tokenizer.nextToken();
            System.out.println("ttype: " + tokenizer.ttype);
            System.out.println(tokenizer.sval);
            System.out.println(tokenizer.nval);
            System.out.println(tokenizer.toString());
            System.out.println();
        }
        System.out.println("================");
    }
}
