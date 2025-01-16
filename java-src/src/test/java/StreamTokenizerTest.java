import java.util.*;

import static org.junit.jupiter.api.Assertions.assertEquals;

import java.io.*;
import org.junit.jupiter.api.Test;

public class StreamTokenizerTest {
    // @Test
    // public void testStreamTokenizer() throws Exception {
    //     assertDoesNotThrow(() -> streamTokenizerPrintln("1"));
    //     assertDoesNotThrow(() -> streamTokenizerPrintln("\"1\""));
    //     assertDoesNotThrow(() -> streamTokenizerPrintln("'1'"));
    //     assertDoesNotThrow(() -> streamTokenizerPrintln("'1"));
    //     assertDoesNotThrow(() -> streamTokenizerPrintln("'1\" '"));
    // }

    @Test
    void testStreamTokenizer() throws Exception {
        List<String> results = streamTokenizerPrintln("1");
        // assert list and list
        List<String> expected = Arrays.asList("1.0");
        assertEquals(expected, results);
    }
    
    private List<String> streamTokenizerPrintln(String s) throws Exception {
        StringReader reader = new StringReader(s);
        StreamTokenizer tokenizer = new StreamTokenizer(reader);
        // i want result of this method to be printed
        List<String> results = new ArrayList<String>();
        while (tokenizer.ttype != StreamTokenizer.TT_EOF) {
            tokenizer.nextToken();
            results.add(tokenizer.toString());
        }

        return results;
    }
}
